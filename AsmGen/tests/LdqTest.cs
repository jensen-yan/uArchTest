using System.Text;

namespace AsmGen
{
    public class LdqTest : UarchTest
    {
        public LdqTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "ldq";
            this.Description = "Load Queue Test (loads pending retire)";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledLoads = new string[4];
            unrolledLoads[0] = "  mov 8(%rdx), %r15";
            unrolledLoads[1] = "  mov 8(%rdx), %r14";
            unrolledLoads[2] = "  mov 8(%rdx), %r13";
            unrolledLoads[3] = "  mov 8(%rdx), %r12";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledLoads, unrolledLoads, true);
        }
        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledLoads = new string[4];
            unrolledLoads[0] = "  ldr x15, [x1, #8]";
            unrolledLoads[1] = "  ldr x14, [x1, #8]";
            unrolledLoads[2] = "  ldr x13, [x1, #8]";
            unrolledLoads[3] = "  ldr x12, [x1, #8]";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledLoads, unrolledLoads, true);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  ld t5, 8(a1)";
            dependentLoads[1] = "  ld t4, 8(a1)";
            dependentLoads[2] = "  ld t3, 8(a1)";
            dependentLoads[3] = "  ld t2, 8(a1)";

            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads, true);
        }
    }
}
