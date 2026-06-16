using System.Text;

namespace AsmGen
{
    public class LoadLatency : UarchTest
    {
        public LoadLatency(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "loadlat";
            this.Description = $"load chain for measuring latency";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t* arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {

            string initInstrs = "  lea 0x8(%rdx), %rdi\n" + 
                                "  mov %rdi, (%rdi)\n";

            string[] basicBlock = new string[4];
            basicBlock[0] = "  mov (%rdi), %rdi";
            basicBlock[1] = "  mov (%rdi), %rdi";
            basicBlock[2] = "  mov (%rdi), %rdi";
            basicBlock[3] = "  mov (%rdi), %rdi";

            UarchTestHelpers.GenerateX86AsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false, initInstrs);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string initInstrs = "  add x25, x1, 0x8\n" + 
                                "  str x25, [x25]\n";

            string[] basicBlock = new string[4];
            basicBlock[0] = "  ldr x25, [x25]";
            basicBlock[1] = "  ldr x25, [x25]";
            basicBlock[2] = "  ldr x25, [x25]";
            basicBlock[3] = "  ldr x25, [x25]";

            UarchTestHelpers.GenerateArmAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false, initInstrs);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {

            string initInstrs = "  add s1, a1, 0x8\n" + 
                                "  sd s1, 0(s1)\n";

            string[] basicBlock = new string[4];
            basicBlock[0] = "  ld s1, 0(s1)";
            basicBlock[1] = "  ld s1, 0(s1)";
            basicBlock[2] = "  ld s1, 0(s1)";
            basicBlock[3] = "  ld s1, 0(s1)";

            UarchTestHelpers.GenerateRiscvAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false, initInstrs);
        }
    }
}
