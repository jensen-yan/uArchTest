using System.Text;

namespace AsmGen
{
    public class FmulSchedTest : UarchTest
    {
        public FmulSchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "fmulsched";
            this.Description = "FP (32-bit multiply) Scheduler Capacity Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            // xmm0 is dependent on ptr chasing load
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  mulss %xmm0, %xmm1";
            unrolledAdds[1] = "  mulss %xmm0, %xmm2";
            unrolledAdds[2] = "  mulss %xmm0, %xmm3";
            unrolledAdds[3] = "  mulss %xmm0, %xmm4";

            UarchTestHelpers.GenerateX86AsmFpSchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  fmul s17, s17, s16";
            unrolledAdds[1] = "  fmul s18, s18, s16";
            unrolledAdds[2] = "  fmul s19, s19, s16";
            unrolledAdds[3] = "  fmul s20, s20, s16";
            UarchTestHelpers.GenerateArmAsmFpSchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  fmul.s f1, f1, f0";
            unrolledAdds[1] = "  fmul.s f2, f2, f0";
            unrolledAdds[2] = "  fmul.s f3, f3, f0";
            unrolledAdds[3] = "  fmul.s f4, f4, f0";
            UarchTestHelpers.GenerateRiscvAsmFpSchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
        }
    }
}
