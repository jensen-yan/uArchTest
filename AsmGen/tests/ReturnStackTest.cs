using System.Text;

namespace AsmGen
{
    public class ReturnStackTest : UarchTest
    {
        public ReturnStackTest(int low, int high, int step)
        {
            this.Counts = UarchTestHelpers.GenerateCountArray(low, high, step);
            this.Prefix = "returnstack";
            this.Description = "Return Stack Depth Test";
            this.FunctionDefinitionParameters = "uint64_t iterations";
            this.GetFunctionCallParameters = "iterations";
            this.DivideTimeByCount = true;
        }

        public override void GenerateX86GccAsm(StringBuilder sb)
        {
            for (int countIdx = 0; countIdx < this.Counts.Length; countIdx++)
            {
                int callDepth = this.Counts[countIdx];
                string topLevelFunctionLabel = this.Prefix + callDepth;
                sb.AppendLine($"{topLevelFunctionLabel}:");
                sb.AppendLine("  xor %rax, %rax");
                sb.AppendLine($"{topLevelFunctionLabel}_loop:");
                sb.AppendLine($"  call " + GetFunctionName(callDepth, 0));
                sb.AppendLine($"  dec %rdi");
                sb.AppendLine($"  jne {topLevelFunctionLabel}_loop");
                sb.AppendLine("  ret");

                // generate a batch of functions so we aren't returning to the same address
                // otherwise a simple predictor will suffice
                for (int callIdx = 0; callIdx < callDepth; callIdx++)
                {
                    string funcName = GetFunctionName(callDepth, callIdx);
                    sb.AppendLine($".global {funcName}");
                    sb.AppendLine($"{funcName}:");
                    if (callIdx < callDepth - 1)
                    {
                        sb.AppendLine($"  add %rdi, %rax");
                        sb.AppendLine("  call " + GetFunctionName(callDepth, callIdx + 1));
                    }

                    sb.AppendLine("  ret");
                }
            }
        }

        public override void GenerateArmAsm(StringBuilder sb)
        {
            for (int countIdx = 0; countIdx < this.Counts.Length; countIdx++)
            {
                int callDepth = this.Counts[countIdx];
                string topLevelFunctionLabel = this.Prefix + callDepth;
                sb.AppendLine($"{topLevelFunctionLabel}:");
                sb.AppendLine("  sub sp, sp, #0x20");
                sb.AppendLine("  stp x29, x30, [sp, #0x10]");
                sb.AppendLine("  eor x3, x3, x3");
                sb.AppendLine($"{topLevelFunctionLabel}_loop:");
                sb.AppendLine($"  bl " + GetFunctionName(callDepth, 0));
                sb.AppendLine("  sub x0, x0, 1");
                sb.AppendLine($"  cbnz x0, {topLevelFunctionLabel}_loop");
                sb.AppendLine("  ldp x29, x30, [sp, #0x10]");
                sb.AppendLine("  add sp, sp, #0x20");
                sb.AppendLine("  ret");

                for (int callIdx = 0; callIdx < callDepth; callIdx++)
                {
                    string funcName = GetFunctionName(callDepth, callIdx);
                    sb.AppendLine($".global {funcName}");
                    sb.AppendLine($"{funcName}:");
                    sb.AppendLine($"  add x3, x3, x0");
                    if (callIdx < callDepth - 1)
                    {
                        // 'bl' is like x86 'call', except it's like the kid that falls asleep in the middle of class
                        // it doesn't push the return address, so you have to do that yourself
                        sb.AppendLine("  sub sp, sp, #0x20");
                        sb.AppendLine("  stp x29, x30, [sp, #0x10]");
                        sb.AppendLine("  bl " + GetFunctionName(callDepth, callIdx + 1));
                        sb.AppendLine("  ldp x29, x30, [sp, #0x10]");
                        sb.AppendLine("  add sp, sp, #0x20");
                    }

                    sb.AppendLine("  ret");
                }
            }
        }

        public override void GenerateRiscvAsm(StringBuilder sb)
        {
            for (int countIdx = 0; countIdx < this.Counts.Length; countIdx++)
            {
                int callDepth = this.Counts[countIdx];
                string topLevelFunctionLabel = this.Prefix + callDepth;
                sb.AppendLine($"{topLevelFunctionLabel}:");
                sb.AppendLine("  addi sp, sp, -32");           // Allocate 32 bytes on the stack
                sb.AppendLine("  sd ra, 24(sp)");              // Save return address (ra)
                sb.AppendLine("  sd s0, 16(sp)");              // Save frame pointer (s0)
                sb.AppendLine("  mv s0, sp");                  // Set the frame pointer
                sb.AppendLine("  mv t0, zero");                // t0 = 0 (equivalent to x3)
                sb.AppendLine($"{topLevelFunctionLabel}_loop:");
                sb.AppendLine($"  call " + GetFunctionName(callDepth, 0)); // Call the first function
                sb.AppendLine("  addi a0, a0, -1");            // Decrement a0 (loop counter)
                sb.AppendLine($"  bnez a0, {topLevelFunctionLabel}_loop"); // Repeat if a0 != 0
                sb.AppendLine("  ld ra, 24(sp)");              // Restore return address
                sb.AppendLine("  ld s0, 16(sp)");              // Restore frame pointer
                sb.AppendLine("  addi sp, sp, 32");            // Deallocate stack
                sb.AppendLine("  ret");

                for (int callIdx = 0; callIdx < callDepth; callIdx++)
                {
                    string funcName = GetFunctionName(callDepth, callIdx);
                    sb.AppendLine($".global {funcName}");
                    sb.AppendLine($"{funcName}:");
                    sb.AppendLine($"  add t0, t0, a0");          // t0 = t0 + a0
                    if (callIdx < callDepth - 1)
                    {
                        sb.AppendLine("  addi sp, sp, -32");     // Allocate stack space
                        sb.AppendLine("  sd ra, 24(sp)");        // Save return address
                        sb.AppendLine("  sd s0, 16(sp)");        // Save frame pointer
                        sb.AppendLine("  mv s0, sp");            // Set frame pointer
                        sb.AppendLine("  call " + GetFunctionName(callDepth, callIdx + 1)); // Call next function
                        sb.AppendLine("  ld ra, 24(sp)");        // Restore return address
                        sb.AppendLine("  ld s0, 16(sp)");        // Restore frame pointer
                        sb.AppendLine("  addi sp, sp, 32");      // Deallocate stack
                    }
                    sb.AppendLine("  ret");
                }
            }
        }

        private string GetFunctionName(int count, int depth) { return $"returnstack{count}_{depth}"; }
    }
}
