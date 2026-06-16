using System.Text;

namespace AsmGen
{
    public class FpRfTest : UarchTest
    {
        public FpRfTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "fprf";
            this.Description = "FP (64-bit scalar) RF Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  addss %xmm1, %xmm2";
            unrolledAdds[1] = "  addss %xmm1, %xmm3";
            unrolledAdds[2] = "  addss %xmm1, %xmm4";
            unrolledAdds[3] = "  addss %xmm1, %xmm5";
            UarchTestHelpers.GenerateX86AsmFpSchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  fadd s18, s18, s17";
            unrolledAdds[1] = "  fadd s19, s19, s17";
            unrolledAdds[2] = "  fadd s20, s20, s17";
            unrolledAdds[3] = "  fadd s21, s21, s17";
            UarchTestHelpers.GenerateArmAsmFpSchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  fadd.s f2, f2, f1";
            unrolledAdds[1] = "  fadd.s f3, f3, f1";
            unrolledAdds[2] = "  fadd.s f4, f4, f1";
            unrolledAdds[3] = "  fadd.s f5, f5, f1";
            UarchTestHelpers.GenerateRiscvAsmFpSchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
        }
    }
}
