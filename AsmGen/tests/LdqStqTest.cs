using System.Text;

namespace AsmGen
{
    public class LdqStqTest : UarchTest
    {
        public LdqStqTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "mixldqstq";
            this.Description = "Mixed Load/Store Queue Test (mem ops pending retire)";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] instrs = new string[4];
            instrs[0] = "  mov %r15, 8(%rdx)";
            instrs[1] = "  mov (%rdx), %r14";
            instrs[2] = "  mov %r13, 8(%rdx)";
            instrs[3] = "  mov (%rdx), %r12";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, instrs, instrs, true);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] instrs = new string[4];
            instrs[0] = "  str x15, [x1, #8]";
            instrs[1] = "  ldr x14, [x1]";
            instrs[2] = "  str x13, [x1, #8]";
            instrs[3] = "  ldr x12, [x1]";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, instrs, instrs, true);
        }


        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] dependentLoads = new string[4];
            dependentLoads[0] = "  sd t5, 8(a1)";
            dependentLoads[1] = "  ld t4, 0(a1)";
            dependentLoads[2] = "  sd t3, 8(a1)";
            dependentLoads[3] = "  ld t2, 0(a1)";

            UarchTestHelpers.GenerateRiscvAsmStructureTestFuncs(sb, this.Counts, this.Prefix, dependentLoads, dependentLoads, true);
        }
    }
}
