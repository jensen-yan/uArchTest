using System.Text;

namespace AsmGen
{
    public class Simd512RfTest : UarchTest
    {
        public Simd512RfTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "simd512rf";
            this.Description = "Simd (512-bit packed fp) RF Test - x86 only";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  vaddps %zmm1, %zmm2, %zmm2";
            unrolledAdds[1] = "  vaddps %zmm1, %zmm3, %zmm3";
            unrolledAdds[2] = "  vaddps %zmm1, %zmm4, %zmm4";
            unrolledAdds[3] = "  vaddps %zmm1, %zmm5, %zmm5";
            UarchTestHelpers.GenerateX86AsmFp512SchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateStub(sb, this.Counts, this.Prefix);
        }

        // TODO: Riscv    
        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateStub(sb, this.Counts, this.Prefix);
        }
    }
}
