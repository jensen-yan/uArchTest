using System.Text;

namespace AsmGen
{
    public class StqTest : UarchTest
    {
        public StqTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "stq";
            this.Description = "Store Queue Test (stores pending retire)";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledLoads = new string[4];
            unrolledLoads[0] = "  mov %r15, 8(%rdx)";
            unrolledLoads[1] = "  mov %r14, 8(%rdx)";
            unrolledLoads[2] = "  mov %r13, 8(%rdx)";
            unrolledLoads[3] = "  mov %r12, 8(%rdx)";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledLoads, unrolledLoads, true);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledLoads = new string[4];
            unrolledLoads[0] = "  str x15, [x25,#8]";
            unrolledLoads[1] = "  str x14, [x25,#8]";
            unrolledLoads[2] = "  str x13, [x25,#8]";
            unrolledLoads[3] = "  str x12, [x25,#8]";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledLoads, unrolledLoads, true);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  sd t5, 8(a1)";
            dependentLoads[1] = "  sd t4, 8(a1)";
            dependentLoads[2] = "  sd t3, 8(a1)";
            dependentLoads[3] = "  sd t2, 8(a1)";

            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads, true);
        }
    }
}
