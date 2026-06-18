using System.Text;

namespace AsmGen
{
    public class RobTest : UarchTest
    {
        private string[] nops;

        public RobTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "rob";
            this.Description = "Reorder Buffer Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
            this.nops = new string[] { "nop" };
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, nops, nops, true);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, nops, nops, true);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] robFillers = new string[] { "  add x0, t5, t4" };
            UarchTestHelpers.GenerateRiscvAsmBlockedStructureTestFuncs(
                sb, this.Counts, this.Prefix, robFillers, null, 12);
        }
    }
}
