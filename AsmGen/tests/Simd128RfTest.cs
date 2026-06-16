using System.Text;

namespace AsmGen
{
    public class Simd128RfTest : UarchTest
    {
        public Simd128RfTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "simd128rf";
            this.Description = "Simd (128-bit packed int) RF Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            // it's ok, the ptr chasing arr should be way bigger than this
            string initInstrs = "  movdqu (%rdx), %xmm1\n" +
                "  movdqu 16(%rdx), %xmm2\n" +
                "  movdqu 32(%rdx), %xmm3\n" +
                "  movdqu 48(%rdx), %xmm4\n" +
                "  movdqu 64(%rdx), %xmm5\n";

            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  paddq %xmm1, %xmm2";
            unrolledAdds[1] = "  paddq %xmm1, %xmm3";
            unrolledAdds[2] = "  paddq %xmm1, %xmm4";
            unrolledAdds[3] = "  paddq %xmm1, %xmm5";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds, false, initInstrs);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string initInstrs = "  ldr q0, [x1]\n" +
                "  ldr q1, [x1, #0x10]\n" +
                "  ldr q2, [x1, #0x20]\n" +
                "  ldr q3, [x1, #0x30]\n" +
                "  ldr q4, [x1, #0x40]\n";

            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  add v1.4s, v1.4s, v0.4s";
            unrolledAdds[1] = "  add v2.4s, v2.4s, v0.4s";
            unrolledAdds[2] = "  add v3.4s, v3.4s, v0.4s";
            unrolledAdds[3] = "  add v4.4s, v4.4s, v0.4s";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds, false, initInstrs);
        }

        // TODO: Riscv    
        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateStub(sb, this.Counts, this.Prefix);
        }
    }
}
