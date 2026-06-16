using System.Text;

namespace AsmGen
{
    public class MulThroughputTest : UarchTest
    {
        public MulThroughputTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mulbw";
            this.Description = $"mul throughput for various loop sizes";
            this.FunctionDefinitionParameters = "uint64_t iterations";
            this.GetFunctionCallParameters = "iterations";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[6];
            basicBlock[0] = "  imul %rdi, %r15";
            basicBlock[1] = "  imul %rdi, %r14";
            basicBlock[2] = "  imul %rdi, %r13";
            basicBlock[3] = "  imul %rdi, %r12";
            basicBlock[4] = "  imul %rdi, %r11";
            basicBlock[5] = "  imul %rdi, %r10";

            UarchTestHelpers.GenerateX86AsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[6];
            basicBlock[0] = "  mul x15, x25, x25";
            basicBlock[1] = "  mul x14, x25, x25";
            basicBlock[2] = "  mul x13, x25, x25";
            basicBlock[3] = "  mul x12, x25, x25";
            basicBlock[4] = "  mul x11, x25, x25";
            basicBlock[5] = "  mul x10, x25, x25";

            UarchTestHelpers.GenerateArmAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[6];
            basicBlock[0] = "  mul t5, s1, s1";
            basicBlock[1] = "  mul t4, s1, s1";
            basicBlock[2] = "  mul t3, s1, s1";
            basicBlock[3] = "  mul t2, s1, s1";
            basicBlock[4] = "  mul t1, s1, s1";
            basicBlock[5] = "  mul t0, s1, s1";

            UarchTestHelpers.GenerateRiscvAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }
    }
}
