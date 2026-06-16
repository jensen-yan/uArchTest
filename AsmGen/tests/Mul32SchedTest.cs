using System.Text;

namespace AsmGen
{
    public class Mul32SchedTest : UarchTest
    {
        public Mul32SchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mul32sched";
            this.Description = "Integer (32-bit mul) Scheduler Capacity Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            // trying to unsuccessfully counter some weird behavior on zhaoxin
            string resetMulsInstr = "mov $11, %r15\n  mov $13, %r14\n  mov $15, %r13\n  mov $17, %r12\n";
            string[] unrolledMuls = new string[4];
            unrolledMuls[0] = "  imul %edi, %r15d";
            unrolledMuls[1] = "  imul %edi, %r14d";
            unrolledMuls[2] = "  imul %edi, %r13d";
            unrolledMuls[3] = "  imul %edi, %r12d";

            string[] unrolledMuls1 = new string[4];
            unrolledMuls1[0] = "  imul %esi, %r15d";
            unrolledMuls1[1] = "  imul %esi, %r14d";
            unrolledMuls1[2] = "  imul %esi, %r13d";
            unrolledMuls1[3] = "  imul %esi, %r12d";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledMuls, unrolledMuls1, false, postLoadInstrs1: resetMulsInstr, postLoadInstrs2: resetMulsInstr);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledMuls = new string[4];
            unrolledMuls[0] = "  mul w15, w15, w25";
            unrolledMuls[1] = "  mul w14, w14, w25";
            unrolledMuls[2] = "  mul w13, w13, w25";
            unrolledMuls[3] = "  mul w12, w12, w25";

            string[] unrolledMuls1 = new string[4];
            unrolledMuls1[0] = "  mul w15, w15, w26";
            unrolledMuls1[1] = "  mul w14, w14, w26";
            unrolledMuls1[2] = "  mul w13, w13, w26";
            unrolledMuls1[3] = "  mul w12, w12, w26";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledMuls, unrolledMuls1, false);
        }

        // TODO: Riscv    
        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateStub(sb, this.Counts, this.Prefix);
        }
    }
}
