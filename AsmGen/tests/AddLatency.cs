using System.Text;

namespace AsmGen
{
    public class AddLatency : UarchTest
    {
        public AddLatency(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "addlat";
            this.Description = $"add chain for measuring latency";
            this.FunctionDefinitionParameters = "uint64_t iterations";
            this.GetFunctionCallParameters = "iterations";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  add %rdi, %rdi";
            basicBlock[1] = "  add %rdi, %rdi";
            basicBlock[2] = "  add %rdi, %rdi";
            basicBlock[3] = "  add %rdi, %rdi";

            UarchTestHelpers.GenerateX86AsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  add x25, x25, x25";
            basicBlock[1] = "  add x25, x25, x25";
            basicBlock[2] = "  add x25, x25, x25";
            basicBlock[3] = "  add x25, x25, x25";

            UarchTestHelpers.GenerateArmAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  add s1, s1, s1";
            basicBlock[1] = "  add s1, s1, s1";
            basicBlock[2] = "  add s1, s1, s1";
            basicBlock[3] = "  add s1, s1, s1";

            UarchTestHelpers.GenerateRiscvAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }
    }
}
