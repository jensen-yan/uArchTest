using System.Text;

namespace AsmGen
{
    public class MemDepPredTest : UarchTest
    {
        public MemDepPredTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mdp";
            this.Description = "Memory dependency predictor capacity test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrollBlocks = new string[1];
            unrollBlocks[0] = "  mov %r14, 8(%rdx,%rsi)\n" + 
                              "  mov 8(%rdx), %rsi";

            string postLoadInstrs = "  mov 8(%rdx), %rsi";
            UarchTestHelpers.GenerateX86AsmThroughputTestFuncs(
                sb, this.Counts, this.Prefix, unrollBlocks, unrollBlocks, false, null, postLoadInstrs);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrollBlocks = new string[1];
            unrollBlocks[0] = "  add x25, x25, x1\n" + 
                              "  str x14, [x25, #8]\n" + 
                              "  ldr x25, [x1, #8]";

            string postLoadInstrs = "  ldr x25, [x1, #8]";
            UarchTestHelpers.GenerateArmAsmThroughputTestFuncs(
                    sb, this.Counts, this.Prefix, unrollBlocks, unrollBlocks, false, null, postLoadInstrs);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] unrollBlocks = new string[4];
            unrollBlocks[0] = "  add s1, s1, a1\n" + 
                              "  sd t4, 8(s1)\n" + 
                              "  ld s1, 8(a1)";

            string postLoadInstrs = "  ld s1, 8(a1)";
            UarchTestHelpers.GenerateRiscvAsmThroughputTestFuncs(
                sb, this.Counts, this.Prefix, unrollBlocks, unrollBlocks, false, null, postLoadInstrs);
        }
    }
}
