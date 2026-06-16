using System.Text;

namespace AsmGen
{
    public class MixMaskIntRfTest : UarchTest
    {
        public MixMaskIntRfTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mixmaskintrf";
            this.Description = "Mixed Integer and Mask (K regs) RF Test - AVX-512 x86 CPUs only";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  kaddb %k0, %k1, %k1";
            unrolledAdds[1] = "  add %r14, %r13";
            unrolledAdds[2] = "  kaddb %k0, %k3, %k3";
            unrolledAdds[3] = "  add %r11, %r12";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds, false);
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
