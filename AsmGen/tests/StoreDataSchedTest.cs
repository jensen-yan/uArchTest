using System.Text;

namespace AsmGen
{
    public class StoreDataSchedTest : UarchTest
    {
        public StoreDataSchedTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "storedatasched";
            this.Description = "Store Data Scheduler (assuming load hoisting) Capacity Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr1";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] dependentStores = new string[4];
            dependentStores[0] = "  mov %rdi, 8(%rdx)";
            dependentStores[1] = "  mov %rdi, 8(%rdx)";
            dependentStores[2] = "  mov %rdi, 8(%rdx)";
            dependentStores[3] = "  mov %rdi, 8(%rdx)";

            string[] dependentStores1 = new string[4];
            dependentStores1[0] = "  mov %rsi, 8(%rdx)";
            dependentStores1[1] = "  mov %rsi, 8(%rdx)";
            dependentStores1[2] = "  mov %rsi, 8(%rdx)";
            dependentStores1[3] = "  mov %rsi, 8(%rdx)";

            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentStores, dependentStores1, false);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] dependentStores = new string[4];
            dependentStores[0] = "  str x25, [x1, #8]";
            dependentStores[1] = "  str x25, [x1, #8]";
            dependentStores[2] = "  str x25, [x1, #8]";
            dependentStores[3] = "  str x25, [x1, #8]";

            string[] dependentStores1 = new string[4];
            dependentStores1[0] = "  str x26, [x1, #8]";
            dependentStores1[1] = "  str x26, [x1, #8]";
            dependentStores1[2] = "  str x26, [x1, #8]";
            dependentStores1[3] = "  str x26, [x1, #8]";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentStores, dependentStores1, false);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  sd s1, 8(a1)";
            dependentLoads[1] = "  sd s1, 8(a1)";
            dependentLoads[2] = "  sd s1, 8(a1)";
            dependentLoads[3] = "  sd s1, 8(a1)";

            string[] dependentLoads1 = new string[4];
            dependentLoads1[0] = "  sd s2, 8(a1)";
            dependentLoads1[1] = "  sd s2, 8(a1)";
            dependentLoads1[2] = "  sd s2, 8(a1)";
            dependentLoads1[3] = "  sd s2, 8(a1)";
            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads1, false);
        }
    }
}
