using System.Text;

namespace AsmGen
{
    public class MulSchedTest : UarchTest
    {
        public MulSchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mulsched";
            this.Description = "Integer (64-bit mul) Scheduler Capacity Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledMuls = new string[4];
            unrolledMuls[0] = "  imul %rdi, %r15";
            unrolledMuls[1] = "  imul %rdi, %r14";
            unrolledMuls[2] = "  imul %rdi, %r13";
            unrolledMuls[3] = "  imul %rdi, %r12";

            string[] unrolledMuls1 = new string[4];
            unrolledMuls1[0] = "  imul %rsi, %r15";
            unrolledMuls1[1] = "  imul %rsi, %r14";
            unrolledMuls1[2] = "  imul %rsi, %r13";
            unrolledMuls1[3] = "  imul %rsi, %r12";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledMuls, unrolledMuls1, false);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledMuls = new string[4];
            unrolledMuls[0] = "  mul x10, x10, x25";
            unrolledMuls[1] = "  mul x14, x14, x25";
            unrolledMuls[2] = "  mul x13, x13, x25";
            unrolledMuls[3] = "  mul x12, x12, x25";

            string[] unrolledMuls1 = new string[4];
            unrolledMuls1[0] = "  mul x10, x10, x26";
            unrolledMuls1[1] = "  mul x14, x14, x26";
            unrolledMuls1[2] = "  mul x13, x13, x26";
            unrolledMuls1[3] = "  mul x12, x12, x26";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledMuls, unrolledMuls1, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] unrolledMuls = new string[4];
            unrolledMuls[0] = "  mul t0, t0, s1";
            unrolledMuls[1] = "  mul t1, t1, s1";
            unrolledMuls[2] = "  mul t2, t2, s1";
            unrolledMuls[3] = "  mul t3, t3, s1";

            string[] unrolledMuls1 = new string[4];
            unrolledMuls1[0] = "  mul t0, t0, s2";
            unrolledMuls1[1] = "  mul t1, t1, s2";
            unrolledMuls1[2] = "  mul t2, t2, s2";
            unrolledMuls1[3] = "  mul t3, t3, s2";

            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledMuls, unrolledMuls1, false);
        }

    }
}
