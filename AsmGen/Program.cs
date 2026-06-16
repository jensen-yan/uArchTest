using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace AsmGen
{
    class Program
    {
        public static string DataFilesDir = "DataFiles";

        static int structTestIterations = 50000;
        static int throughputTestIterations = 20;

        // Binary data structure: 64 bytes per element
        // [0-7]:   uint64_t offset to next element (64B aligned)
        // [8-15]:  uint64_t index value
        // [16-19]: float value (index + 0.1)
        // [20-63]: padding (reserved for future use)
        static int elementSize = 64; // element size must be 32 * 2^n for some n
        static int latencyListSize = 128 * 1024 * 1024 / elementSize; // 128 MB
        

        static void Main(string[] args)
        {
            // Parse command line arguments
            bool generateSeparateTests = false;
            string targetArch = "all"; // Default to all architectures
            string timingMode = "ns"; // Default to nanosecond timing
            bool useKlib = false; // Default to standard library
            
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--separate" || args[i] == "-s")
                {
                    generateSeparateTests = true;
                }
                else if (args[i] == "--target-arch" && i + 1 < args.Length)
                {
                    targetArch = args[i + 1].ToLower();
                    i++; // Skip the next argument as it's the architecture value
                }
                else if (args[i] == "--timing" && i + 1 < args.Length)
                {
                    timingMode = args[i + 1].ToLower();
                    i++; // Skip the next argument as it's the timing mode value
                }
                else if (args[i] == "--use-klib")
                {
                    useKlib = true;
                }
                else if (args[i] == "--help" || args[i] == "-h")
                {
                    PrintUsage();
                    return;
                }
            }
            
            // Validate architecture
            if (!IsValidArchitecture(targetArch))
            {
                Console.WriteLine($"Error: Invalid architecture '{targetArch}'. Valid options: all, x86, arm, riscv");
                PrintUsage();
                return;
            }
            
            // Validate timing mode
            if (!IsValidTimingMode(timingMode))
            {
                Console.WriteLine($"Error: Invalid timing mode '{timingMode}'. Valid options: ns, cycle");
                PrintUsage();
                return;
            }
            
            Console.WriteLine($"Mode: {(generateSeparateTests ? "Separate tests" : "Combined tests")}");
            Console.WriteLine($"Target Architecture: {targetArch}");
            Console.WriteLine($"Timing Mode: {timingMode}");
            Console.WriteLine($"Library: {(useKlib ? "klib" : "standard library")}");

            List<IUarchTest> tests = new List<IUarchTest>();
            tests.Add(new RobTest(4, 1024, 1));
            tests.Add(new LdqTest(4, 512, 1));
            tests.Add(new StqTest(4, 512, 1));
            tests.Add(new LdqStqTest(4, 512, 1));
            tests.Add(new TakenBranchBufferTest(1, 256, 1));
            tests.Add(new BranchBufferTest(1, 256, 1));
            tests.Add(new MemDepPredTest(4, 128, 1));

            tests.Add(new IntRfTest(4, 512, 1));
            tests.Add(new FpRfTest(4, 512, 1));
            tests.Add(new FlagRfTest(4, 512, 1));
            tests.Add(new Simd128RfTest(4, 512, 1));
            tests.Add(new Simd256RfTest(4, 512, 1));
            tests.Add(new Simd512RfTest(32, 512, 1));

            tests.Add(new AddSchedTest(4, 512, 1));
            tests.Add(new MulSchedTest(4, 512, 1));
            tests.Add(new FaddSchedTest(4, 512, 1));
            tests.Add(new FmulSchedTest(4, 512, 1));
            tests.Add(new JumpSchedTest(4, 128, 1));
            tests.Add(new LoadSchedTest(4, 512, 1));
            tests.Add(new StoreSchedTest(4, 512, 1));
            tests.Add(new StoreDataSchedTest(2, 512, 1));
            tests.Add(new MixLoadStoreSchedTest(4, 512, 1));

            tests.Add(new BtbTest(4, BtbTest.BranchType.Unconditional));
            tests.Add(new BtbTest(8, BtbTest.BranchType.Unconditional));
            tests.Add(new BtbTest(16, BtbTest.BranchType.Unconditional));
            tests.Add(new BtbTest(32, BtbTest.BranchType.Unconditional));
            tests.Add(new BtbTest(64, BtbTest.BranchType.Unconditional));
            tests.Add(new BranchHistoryTest());
            tests.Add(new IndirectBranchTest());
            tests.Add(new ReturnStackTest(1, 256, 1));

            tests.Add(new NopThroughputTest(512, 512, 1));
            tests.Add(new AddThroughputTest(512, 512, 1));
            tests.Add(new MulThroughputTest(512, 512, 1));
            tests.Add(new LoadThroughputTest(512, 512, 1));
            tests.Add(new AddLatency(512, 512, 1));
            tests.Add(new MulLatency(512, 512, 1));
            tests.Add(new LoadLatency(512, 512, 1));

            // Create generate directory
            string generateDir = "generate";
            if (!Directory.Exists(generateDir))
                Directory.CreateDirectory(generateDir);

            // Generate binary data file first
            GenerateBinaryDataFile(generateDir);
            
            // Generate assembly file to include binary data
            GenerateDataAssemblyFile(generateDir);

            if (generateSeparateTests)
            {
                GenerateSeparateTestFiles(tests, generateDir, targetArch, timingMode, useKlib);
            }
            else
            {
                GenerateCombinedTestFiles(tests, generateDir, targetArch, timingMode, useKlib);
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet run [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --separate, -s              Generate separate files for each test");
            Console.WriteLine("  --target-arch <arch>        Target architecture: all, x86, arm, riscv (default: all)");
            Console.WriteLine("  --timing <mode>             Timing mode: ns, cycle (default: ns)");
            Console.WriteLine("  --use-klib                  Use klib.h instead of standard library headers");
            Console.WriteLine("  --help, -h                  Show this help message");
            Console.WriteLine();
            Console.WriteLine("Timing modes:");
            Console.WriteLine("  ns                         Use nanosecond timing with clock_gettime");
            Console.WriteLine("  cycle                      Use cycle-level timing with architecture-specific counters");
            Console.WriteLine();
            Console.WriteLine("Library modes:");
            Console.WriteLine("  default                    Use standard library headers (stdio.h, stdint.h, etc.)");
            Console.WriteLine("  --use-klib                 Use klib.h header instead of standard library");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run                                           # Generate combined tests for all architectures (ns timing, std lib)");
            Console.WriteLine("  dotnet run --target-arch riscv --timing cycle       # Generate combined tests for RISC-V with cycle timing");
            Console.WriteLine("  dotnet run --separate --target-arch x86 --use-klib  # Generate separate tests for x86 with klib");
            Console.WriteLine("  dotnet run --use-klib --timing cycle                # Generate combined tests with klib and cycle timing");
        }

        static bool IsValidArchitecture(string arch)
        {
            return arch == "all" || arch == "x86" || arch == "arm" || arch == "riscv";
        }

        static bool IsValidTimingMode(string timing)
        {
            return timing == "ns" || timing == "cycle";
        }

        static void GenerateIncludeHeaders(StringBuilder sb, bool useKlib)
        {
            if (useKlib)
            {
                sb.AppendLine("#include <klib.h>");
                sb.AppendLine("#define USE_KLIB");
            }
            else
            {
                sb.AppendLine("#include <stdio.h>");
                sb.AppendLine("#include <stdint.h>");
                sb.AppendLine("#include <stdlib.h>");
                sb.AppendLine("#include <string.h>");
            }
            sb.AppendLine("#pragma GCC diagnostic ignored \"-Wattributes\"");
        }

        static void GenerateKlibMakefile(string outputDir, string testName)
        {
            StringBuilder makefile = new StringBuilder();
            makefile.AppendLine($"NAME = {testName}");
            makefile.AppendLine("SRCS = $(shell find -L . -name \"*.c\" -o -name \"*.cpp\" -o -name \"*.S\")");
            makefile.AppendLine("include $(AM_HOME)/Makefile.app");
            
            File.WriteAllText(Path.Combine(outputDir, "Makefile"), makefile.ToString());
        }

        static void GenerateCombinedTestFiles(List<IUarchTest> tests, string outputDir, string targetArch, string timingMode, bool useKlib)
        {
            string commonFunctions = File.ReadAllText($"{DataFilesDir}/CommonFunctions.c");

            // Generate C file (always needed)
            StringBuilder cSourceFile = new StringBuilder();
            
            // Generate include headers based on library choice
            GenerateIncludeHeaders(cSourceFile, useKlib);
            
            // Add timing mode define based on parameter
            if (timingMode == "cycle")
            {
                cSourceFile.AppendLine("#define USE_CYCLE_TIMING");
            }
            else
            {
                cSourceFile.AppendLine("// Using nanosecond timing (default)");
            }
            cSourceFile.AppendLine("");
            cSourceFile.AppendLine(commonFunctions);
            cSourceFile.AppendLine("");
            cSourceFile.AppendLine($"uint64_t throughputTestIterations = {throughputTestIterations}, structIterations = {structTestIterations}, iterations;");
            foreach (IUarchTest test in tests) test.GenerateExternLines(cSourceFile);

            AddCommonInitCode(cSourceFile, tests);

            foreach (IUarchTest test in tests) test.GenerateTestBlock(cSourceFile);
            cSourceFile.AppendLine("  // No need to free - using linked binary data");
            cSourceFile.AppendLine("  return 0; }");
            File.WriteAllText(Path.Combine(outputDir, "clammicrobench.c"), cSourceFile.ToString());

            // Generate Makefile for klib if needed
            if (useKlib)
            {
                GenerateKlibMakefile(outputDir, "clammicrobench");
                Console.WriteLine("Generated Makefile for klib");
            }

            // Generate architecture-specific assembly files
            if (targetArch == "all" || targetArch == "riscv")
            {
                StringBuilder riscvAsmFile = new StringBuilder();
                riscvAsmFile.AppendLine(".option arch, rv64gc\n.text\n");
                GenerateDataSymbols(riscvAsmFile);
                foreach (IUarchTest test in tests) test.GenerateAsmGlobalLines(riscvAsmFile);
                foreach (IUarchTest test in tests) test.GenerateRiscvAsm(riscvAsmFile);
                File.WriteAllText(Path.Combine(outputDir, "clammicrobench_riscv.S"), riscvAsmFile.ToString());
                Console.WriteLine("Generated RISC-V assembly file");
            }

            if (targetArch == "all" || targetArch == "arm")
            {
                StringBuilder armAsmFile = new StringBuilder();
                armAsmFile.AppendLine(".arch armv8-a\n.text\n");
                GenerateDataSymbols(armAsmFile);
                foreach (IUarchTest test in tests) test.GenerateAsmGlobalLines(armAsmFile);
                foreach (IUarchTest test in tests) test.GenerateArmAsm(armAsmFile);
                File.WriteAllText(Path.Combine(outputDir, "clammicrobench_arm.S"), armAsmFile.ToString());
                Console.WriteLine("Generated ARM assembly file");
            }

            if (targetArch == "all" || targetArch == "x86")
            {
                StringBuilder x86AsmFile = new StringBuilder();
                x86AsmFile.AppendLine(".text\n");
                GenerateDataSymbols(x86AsmFile);
                foreach (IUarchTest test in tests) test.GenerateAsmGlobalLines(x86AsmFile);
                foreach (IUarchTest test in tests) test.GenerateX86GccAsm(x86AsmFile);
                File.WriteAllText(Path.Combine(outputDir, "clammicrobench_x86.S"), x86AsmFile.ToString());
                Console.WriteLine("Generated x86 assembly file");
            }
        }

        static void GenerateSeparateTestFiles(List<IUarchTest> tests, string baseOutputDir, string targetArch, string timingMode, bool useKlib)
        {
            string commonFunctions = File.ReadAllText($"{DataFilesDir}/CommonFunctions.c");

            // First generate the common data files in the base output directory
            GenerateBinaryDataFile(baseOutputDir);
            GenerateDataAssemblyFile(baseOutputDir);

            // Use parallel processing for test file generation
            Parallel.ForEach(tests, test =>
            {
                // Create individual directory for each test
                string testDir = Path.Combine(baseOutputDir, test.Prefix);
                if (!Directory.Exists(testDir))
                    Directory.CreateDirectory(testDir);

                // Generate separate C file for each test (always needed)
                StringBuilder cSourceFile = new StringBuilder();
                
                // Generate include headers based on library choice
                GenerateIncludeHeaders(cSourceFile, useKlib);
                
                // Add timing mode define based on parameter
                if (timingMode == "cycle")
                {
                    cSourceFile.AppendLine("#define USE_CYCLE_TIMING");
                }
                else
                {
                    cSourceFile.AppendLine("// Using nanosecond timing (default)");
                }
                cSourceFile.AppendLine("");
                cSourceFile.AppendLine(commonFunctions);
                cSourceFile.AppendLine("");
                cSourceFile.AppendLine($"uint64_t throughputTestIterations = {throughputTestIterations}, structIterations = {structTestIterations}, iterations;");
                test.GenerateExternLines(cSourceFile);

                AddCommonInitCodeForSingleTest(cSourceFile, test);
                test.GenerateCommonTestBlock(cSourceFile);
                cSourceFile.AppendLine("  // No need to free - using linked binary data");
                cSourceFile.AppendLine("  return 0; }");
                
                File.WriteAllText(Path.Combine(testDir, $"{test.Prefix}_test.c"), cSourceFile.ToString());

                // Generate architecture-specific assembly files
                if (targetArch == "all" || targetArch == "riscv")
                {
                    StringBuilder riscvAsmFile = new StringBuilder();
                    riscvAsmFile.AppendLine(".option arch, rv64gc\n.text\n");
                    GenerateDataSymbols(riscvAsmFile);
                    test.GenerateAsmGlobalLines(riscvAsmFile);
                    test.GenerateRiscvAsm(riscvAsmFile);
                    File.WriteAllText(Path.Combine(testDir, $"{test.Prefix}_test_riscv.S"), riscvAsmFile.ToString());
                }

                if (targetArch == "all" || targetArch == "arm")
                {
                    StringBuilder armAsmFile = new StringBuilder();
                    armAsmFile.AppendLine(".arch armv8-a\n.text\n");
                    GenerateDataSymbols(armAsmFile);
                    test.GenerateAsmGlobalLines(armAsmFile);
                    test.GenerateArmAsm(armAsmFile);
                    File.WriteAllText(Path.Combine(testDir, $"{test.Prefix}_test_arm.S"), armAsmFile.ToString());
                }

                if (targetArch == "all" || targetArch == "x86")
                {
                    StringBuilder x86AsmFile = new StringBuilder();
                    x86AsmFile.AppendLine(".text\n");
                    GenerateDataSymbols(x86AsmFile);
                    test.GenerateAsmGlobalLines(x86AsmFile);
                    test.GenerateX86GccAsm(x86AsmFile);
                    File.WriteAllText(Path.Combine(testDir, $"{test.Prefix}_test_x86.S"), x86AsmFile.ToString());
                }

                // Copy the data files to each test directory for easier compilation
                File.Copy(Path.Combine(baseOutputDir, "latency_test_data.bin"), Path.Combine(testDir, "latency_test_data.bin"), true);
                File.Copy(Path.Combine(baseOutputDir, "latency_test_data.S"), Path.Combine(testDir, "latency_test_data.S"), true);

                // Generate Makefile for klib if needed
                if (useKlib)
                {
                    GenerateKlibMakefile(testDir, $"{test.Prefix}_test");
                    Console.WriteLine($"Generated Makefile for klib in {testDir}");
                }

                Console.WriteLine($"Generated separate files for test: {test.Prefix} (arch: {targetArch}) in directory: {testDir}");
            });
        }

        static void AddCommonInitCodeForSingleTest(StringBuilder sb, IUarchTest test)
        {
            sb.AppendLine("int main(int argc, char *argv[]) {");
            sb.AppendLine("  double latency; uint64_t *A = NULL; // B and fpArr are now embedded in A structure");

            // Simple parameter handling for single test - only iterations
            sb.AppendLine($"  printf(\"Usage: {test.Prefix} [iterations = {structTestIterations}] \\n\");");
            sb.AppendLine("#ifndef USE_KLIB");
            if (test.DivideTimeByCount) {
                sb.AppendLine("  if (argc > 1) { throughputTestIterations = atoi(argv[1]);}");
            } else {
                sb.AppendLine("  if (argc > 1) { structIterations = atoi(argv[1]);}");
            }
            sb.AppendLine("#endif");
            
            GenerateLatencyTestArray(sb);
        }

        static void GenerateDataSymbols(StringBuilder sb)
        {
            sb.AppendLine(".global latency_test_data");
            sb.AppendLine(".global latency_test_data_end");
        }

        static void AddCommonInitCode(StringBuilder sb, List<IUarchTest> tests)
        {
            sb.AppendLine("int main(int argc, char *argv[]) {");
            sb.AppendLine("  double latency; uint64_t *A = NULL; // B and fpArr are now embedded in A structure");

            // print a help message based on tests available
            sb.AppendLine($"  printf(\"Usage: [test name] [structTestIterations = {structTestIterations}] \\n\");");
            sb.AppendLine("  if (argc < 2) {");
            sb.AppendLine("    printf(\"List of tests:\\n\");");
            foreach (IUarchTest test in tests) sb.AppendLine($"    printf(\"  {test.Prefix} - {test.Description}\\n\");");
            sb.AppendLine("  }");
            sb.AppendLine("  if (argc > 2) { structIterations = atoi(argv[2]); throughputTestIterations = 100 * structIterations; }");
            sb.AppendLine("  if (argc == 1 || (argc > 1 && strncmp(argv[1], \"branchtest\", 9)) != 0) {");
            GenerateLatencyTestArray(sb);
            sb.AppendLine("  }");
        }

        static void GenerateBinaryDataFile(string outputDir)
        {
            Random rand = new Random();
            byte[] data = new byte[latencyListSize * elementSize];

            // Create an array 0..latencyListSize-1
            int[] indices = new int[latencyListSize];
            for (int i = 0; i < latencyListSize; i++)
            {
                indices[i] = i;
            }

            // Shuffle the array using Fisher–Yates to get a random order
            int iter = latencyListSize;
            while (iter > 1)
            {
                iter--;
                int j = iter - 1 == 0 ? 0 : rand.Next() % (iter - 1);
                int temp = indices[iter];
                indices[iter] = indices[j];
                indices[j] = temp;
            }
            // Fill the binary data
            for (int i = 0; i < latencyListSize; i++)
            {
                int baseOffset = i * elementSize;
                
                // Calculate next element offset (elementSize aligned)
                int nextElementOffset = indices[i] * elementSize;
                
                // Write offset to next element (8 bytes)
                BitConverter.GetBytes((ulong)nextElementOffset).CopyTo(data, baseOffset);
                
                // Write index value (8 bytes)
                BitConverter.GetBytes((ulong)i).CopyTo(data, baseOffset + 8);
                
                // Write float value as double (4 bytes)
                BitConverter.GetBytes((float)(i + 0.1)).CopyTo(data, baseOffset + 16);
                
                // Remaining 40 bytes are padding (already zero-initialized)
            }
            
            // Write binary data file
            File.WriteAllBytes(Path.Combine(outputDir, "latency_test_data.bin"), data);
        }
        
        static void GenerateDataAssemblyFile(string outputDir)
        {
            StringBuilder dataAsm = new StringBuilder();
            dataAsm.AppendLine(".section .data");
            dataAsm.AppendLine(".balign  64"); // 64-byte alignment
            dataAsm.AppendLine("");
            dataAsm.AppendLine(".global latency_test_data");
            dataAsm.AppendLine("latency_test_data:");
            // Use relative path - the binary file is in the same directory as the assembly file
            dataAsm.AppendLine("  .incbin \"latency_test_data.bin\"");
            dataAsm.AppendLine("");
            dataAsm.AppendLine(".global latency_test_data_end");
            dataAsm.AppendLine("latency_test_data_end:");
            
            File.WriteAllText(Path.Combine(outputDir, "latency_test_data.S"), dataAsm.ToString());
        }

        static void GenerateLatencyTestArray(StringBuilder sb)
        {
            // Use external binary data instead of dynamic allocation
            sb.AppendLine("  // Using pre-generated binary data");
            sb.AppendLine("  extern uint8_t latency_test_data[];");
            sb.AppendLine("  extern uint8_t latency_test_data_end[];");
            sb.AppendLine("  A = (uint64_t*)latency_test_data;");
        }
    }
}
