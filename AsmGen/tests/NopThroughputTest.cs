using System.Text;

namespace AsmGen
{
    public class NopThroughputTest : UarchTest
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="low">must be greater than 2</param>
        /// <param name="high"></param>
        /// <param name="step"></param>
        public NopThroughputTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "nopbw";
            this.Description = $"NOP throughput for various loop sizes";
            this.FunctionDefinitionParameters = "uint64_t iterations";
            this.GetFunctionCallParameters = "iterations";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  nop";
            basicBlock[1] = "  nop";
            basicBlock[2] = "  nop";
            basicBlock[3] = "  nop";

            UarchTestHelpers.GenerateX86AsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  nop";
            basicBlock[1] = "  nop";
            basicBlock[2] = "  nop";
            basicBlock[3] = "  nop";

            UarchTestHelpers.GenerateArmAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] basicBlock = new string[4];
            basicBlock[0] = "  nop";
            basicBlock[1] = "  nop";
            basicBlock[2] = "  nop";
            basicBlock[3] = "  nop";
            
            UarchTestHelpers.GenerateRiscvAsmThroughputTestFuncs(sb, this.Counts, this.Prefix, basicBlock, basicBlock, false);
        }
    }
}
