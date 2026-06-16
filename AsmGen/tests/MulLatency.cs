using System.Text;

namespace AsmGen
{
    public class MulLatency : UarchTest
    {
        public MulLatency(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mullat";
            this.Description = $"mul chain for measuring latency";
            this.FunctionDefinitionParameters = "uint64_t iterations";
            this.GetFunctionCallParameters = "iterations";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  imul %rdi, %rdi";
            basicBlock[1] = "  imul %rdi, %rdi";
            basicBlock[2] = "  imul %rdi, %rdi";
            basicBlock[3] = "  imul %rdi, %rdi";

            UarchTestHelpers.GenerateX86AsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  mul x25, x25, x25";
            basicBlock[1] = "  mul x25, x25, x25";
            basicBlock[2] = "  mul x25, x25, x25";
            basicBlock[3] = "  mul x25, x25, x25";

            UarchTestHelpers.GenerateArmAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  mul s1, s1, s1";
            basicBlock[1] = "  mul s1, s1, s1";
            basicBlock[2] = "  mul s1, s1, s1";
            basicBlock[3] = "  mul s1, s1, s1";

            UarchTestHelpers.GenerateRiscvAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }
    }
}
