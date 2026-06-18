using System.Text;

namespace AsmGen
{
    public class IntRfTest : UarchTest
    {
        public IntRfTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "intrf";
            this.Description = "Integer RF Test";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  add %r11, %r15";
            unrolledAdds[1] = "  add %r11, %r14";
            unrolledAdds[2] = "  add %r11, %r13";
            unrolledAdds[3] = "  add %r11, %r12";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds, true);
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  add x15, x15, x11";
            unrolledAdds[1] = "  add x14, x14, x11";
            unrolledAdds[2] = "  add x13, x13, x11";
            unrolledAdds[3] = "  add x12, x12, x11";
            UarchTestHelpers.GenerateArmAsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds, true);
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  add t5, t1, t6";
            unrolledAdds[1] = "  add t4, t1, t6";
            unrolledAdds[2] = "  add t3, t1, t6";
            unrolledAdds[3] = "  add t2, t1, t6";
            UarchTestHelpers.GenerateRiscvAsmBlockedStructureTestFuncs(
                sb, this.Counts, this.Prefix, unrolledAdds, null, 8);
        }
    }
}
