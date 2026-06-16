using System.Text;

namespace AsmGen
{
    public class JumpSchedTest : UarchTest
    {
        public JumpSchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "jmpsched";
            this.Description = "Not-taken Jump Scheduler Capacity Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledJumps = new string[1];
            unrolledJumps[0] = $"  cmp %rdi, %rsi\n  je jumpsched_reallybadthing";

            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledJumps, unrolledJumps, false);

            sb.AppendLine("jumpsched_reallybadthing:");
            sb.AppendLine("  int3");
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledJumps = new string[1];
            unrolledJumps[0] = "  cmp x25, x26\n  b.eq jumpsched_reallybadthing";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(
                sb, this.Counts, this.Prefix, unrolledJumps, unrolledJumps, false);

            sb.AppendLine("jumpsched_reallybadthing:");
            sb.AppendLine("  .word 0xf7f0a000");
        }
        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] unrolledJumps = new string[1];
            unrolledJumps[0] = "  beq s1, s2, jumpsched_reallybadthing";
            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(
                sb, this.Counts, this.Prefix, unrolledJumps, unrolledJumps, false);

            sb.AppendLine("jumpsched_reallybadthing:");
            sb.AppendLine("  .word 0x0000006b");
        }
    }
}
