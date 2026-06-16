using System.Text;

namespace AsmGen
{
    public class StoreSchedTest : UarchTest
    {
        public StoreSchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "storeaddrsched";
            this.Description = "Store Address Scheduler (assuming load hoisting) Capacity Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr1";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] dependentStores = new string[4];
            dependentStores[0] = "  mov %r15, 8(%rdi)";
            dependentStores[1] = "  mov %r15, 8(%rdi)";
            dependentStores[2] = "  mov %r15, 8(%rdi)";
            dependentStores[3] = "  mov %r15, 8(%rdi)";

            string[] dependentStores1 = new string[4];
            dependentStores1[0] = "  mov %r11, 8(%rsi)";
            dependentStores1[1] = "  mov %r11, 8(%rsi)";
            dependentStores1[2] = "  mov %r11, 8(%rsi)";
            dependentStores1[3] = "  mov %r11, 8(%rsi)";

            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentStores, dependentStores1, false);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] dependentStores = new string[4];
            dependentStores[0] = "  str x15, [x25, #8]";
            dependentStores[1] = "  str x15, [x25, #8]";
            dependentStores[2] = "  str x15, [x25, #8]";
            dependentStores[3] = "  str x15, [x25, #8]";

            string[] dependentStores1 = new string[4];
            dependentStores1[0] = "  str x14, [x26, #8]";
            dependentStores1[1] = "  str x14, [x26, #8]";
            dependentStores1[2] = "  str x14, [x26, #8]";
            dependentStores1[3] = "  str x14, [x26, #8]";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentStores, dependentStores1, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  sd t6, 8(s1)";
            dependentLoads[1] = "  sd t6, 8(s1)";
            dependentLoads[2] = "  sd t6, 8(s1)";
            dependentLoads[3] = "  sd t6, 8(s1)";

            string[] dependentLoads1 = new string[4];
            dependentLoads1[0] = "  sd t5, 8(s2)";
            dependentLoads1[1] = "  sd t5, 8(s2)";
            dependentLoads1[2] = "  sd t5, 8(s2)";
            dependentLoads1[3] = "  sd t5, 8(s2)";
            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads1, false);
        }
    }
}
