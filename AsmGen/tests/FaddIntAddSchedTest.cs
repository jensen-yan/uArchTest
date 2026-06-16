using System.Text;

namespace AsmGen
{
    public class FaddIntAddSchedTest : UarchTest
    {
        public FaddIntAddSchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mixfaddintaddsched";
            this.Description = "Mixed FP/Integer Scheduler Capacity Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            // xmm0 is dependent on ptr chasing load
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  addss %xmm0, %xmm1";
            unrolledAdds[1] = "  add %edi, %r11d";
            unrolledAdds[2] = "  addss %xmm0, %xmm3";
            unrolledAdds[3] = "  add %edi, %r12d";

            string[] unrolledAdds1 = new string[4];
            unrolledAdds1[0] = "  addss %xmm0, %xmm1";
            unrolledAdds1[1] = "  add %esi, %r14d";
            unrolledAdds1[2] = "  addss %xmm0, %xmm3";
            unrolledAdds1[3] = "  add %esi, %r15d";

            string rdicvt = "cvtsi2ss %rdi, %xmm0";
            string rsicvt = "cvtsi2ss %rsi, %xmm0";

            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds,
                includePtrChasingLoads: false, postLoadInstrs1: rdicvt, postLoadInstrs2: rsicvt);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  fadd s17, s17, s16";
            unrolledAdds[1] = "  fadd s18, s18, s16";
            unrolledAdds[2] = "  fadd s19, s19, s16";
            unrolledAdds[3] = "  fadd s20, s20, s16";
            UarchTestHelpers.GenerateArmAsmFpSchedTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds);
        }

        // TODO: Riscv    
        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateStub(sb, this.Counts, this.Prefix);
        }
    }
}
