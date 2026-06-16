using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AsmGen
{
    public class TakenBranchBufferTest : UarchTest
    {
        public TakenBranchBufferTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "ftq";
            this.Description = "Fetch Target Queue Test (taken branches pending retire for measuring ftq capacity)";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            void fillerFunc(StringBuilder sb, string funcName, int fillerIdx) 
            {
                string jumpLabel = $"{funcName}_target{fillerIdx}";
                sb.AppendLine($"  jmp {jumpLabel}");
                sb.AppendLine(".balign 64");
                if (fillerIdx % 2 == 0) sb.AppendLine("  nop");
                sb.AppendLine($"{jumpLabel}:");
            }

            UarchTestHelpers.GenerateX86AsmJumpSchedTestFuncs(sb, Counts, Prefix, fillerFunc);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {

            void fillerFunc(StringBuilder sb, string funcName, int fillerIdx) 
            {
                string jumpLabel = $"{funcName}_target{fillerIdx}";
                sb.AppendLine($"  b {jumpLabel}");
                sb.AppendLine(".balign 64");
                sb.AppendLine($"{jumpLabel}:");
            }

            UarchTestHelpers.GenerateArmAsmJumpSchedTestFuncs(sb, Counts, Prefix, fillerFunc);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            void fillerFunc(StringBuilder sb, string funcName, int fillerIdx) 
            {
                string jumpLabel = $"{funcName}_target{fillerIdx}";
                sb.AppendLine($"  j {jumpLabel}");
                sb.AppendLine(".balign 64");
                sb.AppendLine($"{jumpLabel}:");
            }

            UarchTestHelpers.GenerateRiscvAsmJumpSchedTestFuncs(sb, Counts, Prefix, fillerFunc);
        }
    }
}
