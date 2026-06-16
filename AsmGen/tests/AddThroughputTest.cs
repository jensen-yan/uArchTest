using System.Text;

namespace AsmGen
{
    public class AddThroughputTest : UarchTest
    {
        public AddThroughputTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "addbw";
            this.Description = $"add throughput for various loop sizes";
            this.FunctionDefinitionParameters = "uint64_t iterations";
            this.GetFunctionCallParameters = "iterations";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[6];
            basicBlock[0] = "  add %r8, %r15";
            basicBlock[1] = "  add %r8, %r14";
            basicBlock[2] = "  add %r8, %r13";
            basicBlock[3] = "  add %r8, %r12";
            basicBlock[4] = "  add %r8, %r11";
            basicBlock[5] = "  add %r8, %r10";

            UarchTestHelpers.GenerateX86AsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[8];
            basicBlock[0] = "  add x15, x25, x25";
            basicBlock[1] = "  add x14, x25, x25";
            basicBlock[2] = "  add x13, x25, x25";
            basicBlock[3] = "  add x12, x25, x25";
            basicBlock[4] = "  add x11, x25, x25";
            basicBlock[5] = "  add x10, x25, x25";

            UarchTestHelpers.GenerateArmAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[6];
            basicBlock[0] = "  add t5, s1, s1";
            basicBlock[1] = "  add t4, s1, s1";
            basicBlock[2] = "  add t3, s1, s1";
            basicBlock[3] = "  add t2, s1, s1";
            basicBlock[4] = "  add t1, s1, s1";
            basicBlock[5] = "  add t0, s1, s1";

            UarchTestHelpers.GenerateRiscvAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }
    }
}
