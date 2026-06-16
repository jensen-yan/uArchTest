using System.Text;

namespace AsmGen
{
    public class BranchBufferTest : UarchTest
    {
        private bool mixNops;
        public BranchBufferTest(int low, int high, int step, bool mixNops = false)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "brq";
            this.Description = "Branch Reorder Queue Test (not-taken branches pending retire for measuring brq capacity)";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
            this.mixNops = mixNops;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            void fillerFunc(StringBuilder sb, string funcName, int fillerIdx) 
            {
                string jumpLabel = $"{funcName}_target{fillerIdx}";
                sb.AppendLine($"  cmp %r14, %r11");
                sb.AppendLine($"  je {jumpLabel}");
                if (this.mixNops) sb.AppendLine($"  nop");
                // try to space the jumps out a bit
                sb.AppendLine($"{jumpLabel}:");
            }

            UarchTestHelpers.GenerateX86AsmJumpSchedTestFuncs(sb, Counts, Prefix, fillerFunc);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {

            void fillerFunc(StringBuilder sb, string funcName, int fillerIdx) 
            {
                string jumpLabel = $"{funcName}_target{fillerIdx}";
                sb.AppendLine($"  cmp x15, x10");
                sb.AppendLine($"  b.eq {jumpLabel}");
                sb.AppendLine($"{jumpLabel}:");
            }

            UarchTestHelpers.GenerateArmAsmJumpSchedTestFuncs(sb, Counts, Prefix, fillerFunc);
        }
        
        public override void GenerateRiscvAsm(StringBuilder sb)
        {

            void fillerFunc(StringBuilder sb, string funcName, int fillerIdx) 
            {
                string jumpLabel = $"{funcName}_target{fillerIdx}";
                sb.AppendLine($"  beq x15, x10, {jumpLabel}");
                sb.AppendLine($"{jumpLabel}:");
            }

            UarchTestHelpers.GenerateRiscvAsmJumpSchedTestFuncs(sb, Counts, Prefix, fillerFunc);
        }
    }
}
