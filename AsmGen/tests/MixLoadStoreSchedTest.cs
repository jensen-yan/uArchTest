using System.Text;

namespace AsmGen
{
    public class MixLoadStoreSchedTest : UarchTest
    {
        public MixLoadStoreSchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mixloadstoresched";
            this.Description = "Mixed Load/Store (Address Dependency) scheduler capacity test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] ops1 = new string[4];
            ops1[0] = "  mov (%rdi), %r15";
            ops1[1] = "  mov %r11, 8(%rdi)";
            ops1[2] = "  mov (%rdi), %r13";
            ops1[3] = "  mov %r11, 8(%rdi)";

            string[] ops2 = new string[4];
            ops2[0] = "  mov (%rsi), %r15";
            ops2[1] = "  mov %r11, 8(%rsi)";
            ops2[2] = "  mov (%rsi), %r13";
            ops2[3] = "  mov %r11, 8(%rsi)";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, ops1, ops2, true);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  ldr x15, [x25]";
            dependentLoads[1] = "  str x14, [x25, #8]";
            dependentLoads[2] = "  ldr x13, [x25]";
            dependentLoads[3] = "  str x12, [x25, #8]";

            string[] dependentLoads1 = new string[4];
            dependentLoads1[0] = "  ldr x15, [x26]";
            dependentLoads1[1] = "  str x14, [x26, #8]";
            dependentLoads1[2] = "  ldr x13, [x26]";
            dependentLoads1[3] = "  str x12, [x26, #8]";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads1, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  ld t5, 0(s1)";
            dependentLoads[1] = "  sd t4, 8(s1)";
            dependentLoads[2] = "  ld t3, 0(s1)";
            dependentLoads[3] = "  sd t2, 8(s1)";

            string[] dependentLoads1 = new string[4];
            dependentLoads1[0] = "  ld t5, 0(s2)";
            dependentLoads1[1] = "  sd t4, 8(s2)";
            dependentLoads1[2] = "  ld t3, 0(s2)";
            dependentLoads1[3] = "  sd t2, 8(s2)";

            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads1, false);
        }
    }
}
