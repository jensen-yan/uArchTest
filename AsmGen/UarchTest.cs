using System.Text;

namespace AsmGen
{
    public abstract class UarchTest : IUarchTest
    {
        public string Prefix { get; set; }

        public string Description { get; set; }

        public int[] Counts;

        public string FunctionDefinitionParameters { get; set; }

        public string GetFunctionCallParameters { get; set; }

        public bool DivideTimeByCount { get; set; }

        public abstract void GenerateRiscvAsm(StringBuilder sb);

        public abstract void GenerateArmAsm(StringBuilder sb);

        public abstract void GenerateX86GccAsm(StringBuilder sb);

        public void GenerateAsmGlobalLines(StringBuilder sb)
        {
            for (int i = 0; i < Counts.Length; i++)
                sb.AppendLine(".global " + Prefix + Counts[i]);
        }

        public void GenerateExternLines(StringBuilder sb)
        {
            for (int i = 0; i < Counts.Length; i++)
                sb.AppendLine("extern uint64_t " + Prefix + Counts[i] + $"({FunctionDefinitionParameters}) __attribute((sysv_abi));"); ;
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
            // Common test block logic without the argv check - used for standalone tests
            for (int i = 0; i < Counts.Length; i++)
            {
                sb.AppendLine("    {");
                // use more iterations (iterations = structIterations * 100) and divide iteration count by tested-thing count
                // for certain tests like call stack depth
                sb.AppendLine("      iterations = throughputTestIterations;");
                sb.AppendLine("      " + Prefix + Counts[i] + $"({GetFunctionCallParameters});");
                if (DivideTimeByCount) {
                    sb.AppendLine("      iterations = throughputTestIterations * " + Counts[Counts.Length - 1] / (Counts[i]) + ";");
                } else {
                    sb.AppendLine("      iterations = structIterations;");
                }
                sb.AppendLine("      GET_TIME_START();");
                sb.AppendLine("      " + Prefix + Counts[i] + $"({GetFunctionCallParameters});");
                sb.AppendLine("      GET_TIME_END();");
                sb.AppendLine("      CALC_TIME_DIFF();");
                if (DivideTimeByCount)
                    sb.AppendLine("      latency = (double)TIME_DIFF_VALUE / (double)(iterations * " + Counts[i] + ");");
                else
                    sb.AppendLine("      latency = (double)TIME_DIFF_VALUE / (double)(iterations);");
                sb.AppendLine("      printf(\"" + Counts[i] + ",%f \\n\", latency);");

                sb.AppendLine("    }");
            }
        }

    }
}
