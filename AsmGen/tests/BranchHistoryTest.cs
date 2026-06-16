using System.IO;
using System.Text;

namespace AsmGen
{
    public class BranchHistoryTest : IUarchTest
    {
        public string Prefix { get; private set; }

        public string Description { get; private set; }

        public string FunctionDefinitionParameters { get; private set; }

        public string GetFunctionCallParameters { get; private set; }

        public bool DivideTimeByCount { get; private set; }

        private int[] branchCounts;
        private int[] historyCounts;

        public BranchHistoryTest()
        {
            Prefix = "branchhist";
            Description = "Branch predictor pattern recognition";
            FunctionDefinitionParameters = "uint64_t iterations, uint32_t *arr, uint32_t* arrEnd";
            GetFunctionCallParameters = "iterations";
            DivideTimeByCount = true;
            branchCounts = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 };
            historyCounts = new int[] { 2, 4, 8, 12, 16, 24, 32, 48, 64, 96, 128, 192, 256, 512, 600, 768, 1024, 1536,
              2048, 3072, 4096, 5120, 6144, 8192, 10240, 12288, 16384, 24567, 32768, 65536 };
        }

        public void GenerateRiscvAsm(StringBuilder sb)
        {
            for (int i = 0; i < branchCounts.Length; i++)
            {
                string functionLabel = Prefix + branchCounts[i];
                string loopLabel = functionLabel + "_loop";
                sb.AppendLine("\n" + functionLabel + ":");
                sb.AppendLine("  addi sp, sp, -80");  // Adjust stack (16-byte aligned)
                sb.AppendLine("  sd s2, 8(sp)");
                sb.AppendLine("  sd s1, 16(sp)");
                sb.AppendLine("  sd t6, 24(sp)");
                sb.AppendLine("  sd t5, 32(sp)");
                sb.AppendLine("  sd t4, 40(sp)");
                sb.AppendLine("  sd t3, 48(sp)");
                sb.AppendLine("  sd t2, 56(sp)");
                sb.AppendLine("  sd t1, 64(sp)");
                sb.AppendLine("  sd t0, 72(sp)");

                sb.AppendLine("  li t6, 0");
                sb.AppendLine("  li t5, 0");
                sb.AppendLine("  li t4, 0");
                sb.AppendLine("  li t3, 0");
                sb.AppendLine("  li t2, 0");
                sb.AppendLine("  li t1, 0");
                sb.AppendLine("  li t0, 0");
                sb.AppendLine("  li s2, 0");
                sb.AppendLine("  li s1, 0");

                sb.AppendLine("  mv t1, a1");
                sb.AppendLine(".balign 64");
                sb.AppendLine(loopLabel + ":");

                // Generate branch blocks
                for (int branchCount = 0; branchCount < branchCounts[i]; branchCount++)
                {
                    string jumpTarget = functionLabel + branchCounts[i] + "_zero" + branchCount;
                    sb.AppendLine("  lw t3, 0(t1)");
                    sb.AppendLine("  addi t1, t1, 4");
                    sb.AppendLine($"  bnez t3, {jumpTarget}");
                    sb.AppendLine("  addi t0, t0, 1");
                    sb.AppendLine(jumpTarget + ":");
                }
                
                // loop around in pattern history test array if necessary
                // avoiding an extra branch to not pollute BPU history
                sb.AppendLine("  sltu t2, t1, a2");
                sb.AppendLine("  sub t2, x0, t2");      // generate mask filled with 1s if t1 < a2 else 0s
                sb.AppendLine("  xor t1, t1, a1");
                sb.AppendLine("  and t1, t1, t2");
                sb.AppendLine("  xor t1, t1, a1");

                sb.AppendLine("  addi a0, a0, -1");         // Decrement iterations (a0)
                sb.AppendLine($"  bnez a0, {loopLabel}");   // If a0 != 0, loop back

                sb.AppendLine("  mv a0, t0");
                // Restore registers
                sb.AppendLine("  ld s2, 8(sp)");
                sb.AppendLine("  ld s1, 16(sp)");
                sb.AppendLine("  ld t6, 24(sp)");
                sb.AppendLine("  ld t5, 32(sp)");
                sb.AppendLine("  ld t4, 40(sp)");
                sb.AppendLine("  ld t3, 48(sp)");
                sb.AppendLine("  ld t2, 56(sp)");
                sb.AppendLine("  ld t1, 64(sp)");
                sb.AppendLine("  ld t0, 72(sp)");
                sb.AppendLine("  addi sp, sp, 80");  // Adjust stack (16-byte aligned)
                sb.AppendLine("  ret\n\n");
            }
        }

        public void GenerateArmAsm(StringBuilder sb)
        {
            for (int i = 0; i < branchCounts.Length; i++)
            {
                string functionLabel = Prefix + branchCounts[i];
                string loopLabel = functionLabel + "_loop";
                sb.AppendLine("\n" + functionLabel + ":");
                sb.AppendLine("  sub sp, sp, #0x40");
                sb.AppendLine("  stp x11, x12, [sp, #0x30]");
                sb.AppendLine("  stp x15, x16, [sp, #0x20]");
                sb.AppendLine("  stp x13, x14, [sp, #0x10]");
                sb.AppendLine("  eor x16, x16, x16");
                sb.AppendLine("  eor x15, x15, x15");
                sb.AppendLine("  eor x12, x12, x12");
                sb.AppendLine("  eor x11, x11, x11");

                // w14 = branch index, w16 = pattern array index
                sb.AppendLine("  eor w14, w14, w14");
                sb.AppendLine("  mov x16, x1");
                sb.AppendLine(".balign 64");
                sb.AppendLine(loopLabel + ":");

                // generate branch blocks
                for (int branchCount = 0; branchCount < branchCounts[i]; branchCount++)
                {
                    string jumpTarget = functionLabel + branchCounts[i] + "_zero" + branchCount;
                    sb.AppendLine("  ldr w13, [x16]");
                    sb.AppendLine("  add x16, x16, 4");
                    sb.AppendLine($"  cbnz x13, {jumpTarget}");
                    sb.AppendLine("  add x12, x12, 1");
                    sb.AppendLine(jumpTarget + ":");
                }

                // loop around in pattern history test array if necessary
                // avoiding an extra branch to not pollute BPU history
                sb.AppendLine("  cmp x16, x2");
                sb.AppendLine("  csel x16, x1, x16, EQ");
                sb.AppendLine("  sub x0, x0, 1");
                sb.AppendLine($"  cbnz x0, {loopLabel}");
                sb.AppendLine("  mov x0, x12");
                sb.AppendLine("  ldp x11, x12, [sp, #0x30]");
                sb.AppendLine("  ldp x15, x16, [sp, #0x20]");
                sb.AppendLine("  ldp x13, x14, [sp, #0x10]");
                sb.AppendLine("  add sp, sp, #0x40");
                sb.AppendLine("  ret");
            }
        }

        public void GenerateX86GccAsm(StringBuilder sb)
        {
            for (int i = 0; i < branchCounts.Length; i++)
            {
                string functionLabel = Prefix + branchCounts[i];
                sb.AppendLine("\n" + functionLabel + ":");
                sb.AppendLine("  push %rbx");
                sb.AppendLine("  push %r8");
                sb.AppendLine("  push %r9");
                sb.AppendLine("  xor %rbx, %rbx");
                sb.AppendLine("  xor %r8, %r8");
                sb.AppendLine("  xor %r9, %r9");

                sb.AppendLine("  mov %rsi, %r11");
                string loopLabel = functionLabel + "_loop";
                sb.AppendLine(".balign 64");
                sb.AppendLine(loopLabel + ":");
                for (int branchCount = 0; branchCount < branchCounts[i]; branchCount++)
                {
                    sb.AppendLine("  mov (%r11), %eax");  // load array base pointer into r10
                    sb.AppendLine("  add $4, %r11");
                    sb.AppendLine("  test %eax, %eax");

                    // conditional branch on test array value
                    string zeroLabel = Prefix + branchCounts[i] + "_zero" + branchCount;
                    sb.AppendLine("  jz " + zeroLabel);
                    sb.AppendLine("  inc %r8"); // r8 is just a sink here
                    sb.AppendLine(zeroLabel + ":");
                }

                // loop around in pattern history test array if necessary
                // avoiding an extra branch to not pollute BPU history
                sb.AppendLine("  cmp %r11, %rdx");
                sb.AppendLine("  cmove %rsi, %r11");

                // end of main loop over iteration count
                sb.AppendLine("  dec %rdi");
                sb.AppendLine("  jnz " + loopLabel);

                // function epilogue
                sb.AppendLine("  mov %r8, %rax");
                sb.AppendLine("  pop %r9");
                sb.AppendLine("  pop %r8");
                sb.AppendLine("  pop %rbx");
                sb.AppendLine("  ret");
            }
        }

        public void GenerateAsmGlobalLines(StringBuilder sb)
        {
            for (int i = 0; i < branchCounts.Length; i++)
                sb.AppendLine(".global " + Prefix + branchCounts[i]);
        }

        // kinda hack this to put in initialization code we need
        public void GenerateExternLines(StringBuilder sb)
        {
            for (int i = 0; i < branchCounts.Length; i++)
                sb.AppendLine("extern uint64_t " + Prefix + branchCounts[i] + $"({FunctionDefinitionParameters}) __attribute((sysv_abi));");

            GenerateInitializationCode(sb, true);

            string gccFunction = File.ReadAllText($"{Program.DataFilesDir}/GccBranchHistFunction.c");
            sb.AppendLine(gccFunction);
        }


        public void GenerateInitializationCode(StringBuilder sb, bool gcc)
        {
            sb.AppendLine($"uint32_t maxBranchCount = {branchCounts.Length};");
            sb.Append($"uint32_t branchCounts[{branchCounts.Length}] = ");
            sb.Append("{  " + branchCounts[0]);
            for (int i = 1; i < branchCounts.Length; i++) sb.Append(", " + branchCounts[i]);
            sb.AppendLine(" };");
            sb.Append($"uint32_t branchHistoryLengths[{historyCounts.Length}] = ");
            sb.Append("{  " + historyCounts[0]);
            for (int i = 1; i < historyCounts.Length; i++) sb.Append(", " + historyCounts[i]);
            sb.AppendLine(" };");

            if (gcc) sb.AppendLine($"uint64_t (__attribute((sysv_abi)) *branchtestFuncArr[{branchCounts.Length}])(uint64_t iterations, uint32_t *arr, uint32_t* arrEnd);");
            else sb.AppendLine($"uint64_t (*branchtestFuncArr[{branchCounts.Length}])(uint64_t iterations, uint32_t *arr, uint32_t* arrEnd);");

            sb.AppendLine("void initializeBranchHistFuncArr() {");
            for (int i = 0; i < branchCounts.Length; i++)
            {
                sb.AppendLine($"  branchtestFuncArr[{i}] = {Prefix + branchCounts[i]};");
            }

            sb.AppendLine("}");
        }

        public void GenerateTestBlock(StringBuilder sb)
        {
            sb.AppendLine("  if (argc > 1 && strncmp(argv[1], \"" + Prefix + "\", " + Prefix.Length + ") == 0) {");
            GenerateCommonTestBlock(sb);
            sb.AppendLine("  }\n");
        }

        public void GenerateCommonTestBlock(StringBuilder sb)
        {
            sb.AppendLine("    printf(\"" + Description + "(\" TIME_UNIT \"):\\n\");");
            string branchhistMain = File.ReadAllText($"{Program.DataFilesDir}/BranchhistTestBlock.c");
            sb.AppendLine(branchhistMain);
        }
    }
}
