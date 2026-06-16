using System.Text;

namespace AsmGen
{
    public class Simd256RfTest : UarchTest
    {
        public Simd256RfTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "simd256rf";
            this.Description = "Simd (256-bit packed fp) RF Test - x86 only";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  vaddps %ymm1, %ymm2, %ymm2";
            unrolledAdds[1] = "  vaddps %ymm1, %ymm3, %ymm3";
            unrolledAdds[2] = "  vaddps %ymm1, %ymm4, %ymm4";
            unrolledAdds[3] = "  vaddps %ymm1, %ymm5, %ymm5";
            UarchTestHelpers.GenerateX86AsmFp256SchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
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
