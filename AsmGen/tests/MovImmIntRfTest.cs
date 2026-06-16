using System.Text;

namespace AsmGen
{
    public class MovImmIntRfTest : UarchTest
    {
        public MovImmIntRfTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "movimmintrf";
            this.Description = "Integer RF Test (move immediate)";
            this.FunctionDefinitionParameters = "uint64_t iterations, uint64_t *arr";
            this.GetFunctionCallParameters = "iterations, A";
            this.DivideTimeByCount = false;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            string[] unrolledAdds = new string[4];
            unrolledAdds[0] = "  mov $1, %r15";
            unrolledAdds[1] = "  mov $2, %r14";
            unrolledAdds[2] = "  mov $3, %r13";
            unrolledAdds[3] = "  mov $4, %r12";
            UarchTestHelpers.GenerateX86AsmStructureTestFuncs(sb, this.Counts, this.Prefix, unrolledAdds, unrolledAdds, true);
        }


        public override void GenerateArmAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateStub(sb, this.Counts, this.Prefix);
        }

        // TODO: Riscv    
        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            UarchTestHelpers.GenerateStub(sb, this.Counts, this.Prefix);
        }
    }
}
