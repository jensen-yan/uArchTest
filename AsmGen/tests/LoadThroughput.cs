using System.Text;

namespace AsmGen
{
    public class LoadThroughputTest : UarchTest
    {
        public LoadThroughputTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "loadbw";
            this.Description = $"load throughput for various loop sizes";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t* arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[8];
            basicBlock[0] = "  mov 0(%rdx), %r15";
            basicBlock[1] = "  mov 8(%rdx), %r14";
            basicBlock[2] = "  mov 16(%rdx), %r13";
            basicBlock[3] = "  mov 24(%rdx), %r12";
            basicBlock[4] = "  mov 32(%rdx), %r11";
            basicBlock[5] = "  mov 40(%rdx), %r10";
            basicBlock[6] = "  mov 48(%rdx), %rdi";
            basicBlock[7] = "  mov 56(%rdx), %rsi";

            UarchTestHelpers.GenerateX86AsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[8];
            basicBlock[0] = "  ldr x15, [x1, #0]";
            basicBlock[1] = "  ldr x14, [x1, #8]";
            basicBlock[2] = "  ldr x13, [x1, #16]";
            basicBlock[3] = "  ldr x12, [x1, #24]";
            basicBlock[4] = "  ldr x11, [x1, #32]";
            basicBlock[5] = "  ldr x10, [x1, #40]";

            UarchTestHelpers.GenerateArmAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[6];
            basicBlock[0] = "  ld t5, 0(a1)";
            basicBlock[1] = "  ld t4, 8(a1)";
            basicBlock[2] = "  ld t3, 16(a1)";
            basicBlock[3] = "  ld t2, 24(a1)";
            basicBlock[4] = "  ld t1, 32(a1)";
            basicBlock[5] = "  ld t0, 40(a1)";

            UarchTestHelpers.GenerateRiscvAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }
    }
}
