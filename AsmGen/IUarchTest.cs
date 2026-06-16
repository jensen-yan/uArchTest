using System.Text;

namespace AsmGen
{
    public interface IUarchTest
    {
        // enough to generate global lines, function calls, and let user pick from tests
        public string Prefix { get; }
        public string Description { get; }
        public bool DivideTimeByCount { get; }
        public void GenerateX86GccAsm(StringBuilder sb);
        public void GenerateArmAsm(StringBuilder sb);
        public void GenerateRiscvAsm(StringBuilder sb);
        public void GenerateTestBlock(StringBuilder sb);
        public void GenerateCommonTestBlock(StringBuilder sb);

        public void GenerateAsmGlobalLines(StringBuilder sb);

        public void GenerateExternLines(StringBuilder sb);
    }
}
