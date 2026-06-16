using System.Text;

namespace AsmGen
{
    public class LoadSchedTest : UarchTest
    {
        public LoadSchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "loadsched";
            this.Description = "Load scheduler capacity test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  mov (%rdi), %r15";
            dependentLoads[1] = "  mov (%rdi), %r14";
            dependentLoads[2] = "  mov (%rdi), %r13";
            dependentLoads[3] = "  mov (%rdi), %r12";

            string[] dependentLoads1 = new string[4];
            dependentLoads1[0] = "  mov (%rsi), %r15";
            dependentLoads1[1] = "  mov (%rsi), %r14";
            dependentLoads1[2] = "  mov (%rsi), %r13";
            dependentLoads1[3] = "  mov (%rsi), %r12";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads1, false);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  ldr x15, [x25]";
            dependentLoads[1] = "  ldr x14, [x25]";
            dependentLoads[2] = "  ldr x13, [x25]";
            dependentLoads[3] = "  ldr x12, [x25]";

            string[] dependentLoads1 = new string[4];
            dependentLoads1[0] = "  ldr x15, [x26]";
            dependentLoads1[1] = "  ldr x14, [x26]";
            dependentLoads1[2] = "  ldr x13, [x26]";
            dependentLoads1[3] = "  ldr x12, [x26]";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads1, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  ld t5, 0(s1)";
            dependentLoads[1] = "  ld t4, 0(s1)";
            dependentLoads[2] = "  ld t3, 0(s1)";
            dependentLoads[3] = "  ld t2, 0(s1)";

            string[] dependentLoads1 = new string[4];
            dependentLoads1[0] = "  ldr t5, 0(s2)";
            dependentLoads1[1] = "  ldr t4, 0(s2)";
            dependentLoads1[2] = "  ldr t3, 0(s2)";
            dependentLoads1[3] = "  ldr t2, 0(s2)";
            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads1, false);
        }
    }
}
