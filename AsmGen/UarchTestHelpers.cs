using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace AsmGen
{
    public static class UarchTestHelpers
    {

        public static int[] GenerateCountArray(int low, int high, int step)
        {
            List<int> countList = new List<int>();
            for (int i = low; i <= high; i += step)
            {
                countList.Add(i);
            }

            return countList.ToArray();
        }

        public static void GenerateStub(StringBuilder sb, int[] counts, string funcNamePrefix)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                string funcName = funcNamePrefix + counts[i];
                sb.AppendLine("\n" + funcName + ":");
                sb.AppendLine("  ret");
            }
        }
        public static void GenerateX86AsmThroughputTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            string[] fillerInstrs1,
            string[] fillerInstrs2,
            bool includePtrChasingLoads = true,
            string initInstrs = null,
            string postLoadInstrs1 = null,
            string postLoadInstrs2 = null,
            string preLoadInstrs1 = null,
            string preLoadInstrs2 = null,
            bool lfence = false,
            Action<StringBuilder, string, int> fillerFunc1 = null,
            Action<StringBuilder, string, int> fillerFunc2 = null)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                string funcName = funcNamePrefix + counts[i];
                sb.AppendLine("\n" + funcName + ":");
                sb.AppendLine("  push %rsi");
                sb.AppendLine("  push %rdi");
                sb.AppendLine("  push %r15");
                sb.AppendLine("  push %r14");
                sb.AppendLine("  push %r13");
                sb.AppendLine("  push %r12");
                sb.AppendLine("  push %r11");
                sb.AppendLine("  push %r10");
                sb.AppendLine("  push %r8");
                sb.AppendLine("  push %rcx");
                sb.AppendLine("  push %rdx");

                // arguments are in RDI, RSI, RDX, RCX, R8, and R9
                // move them into familiar windows argument regs (rcx, rdx, r8)
                sb.AppendLine("  mov %rdx, %r8"); // r8 <- rdx
                sb.AppendLine("  mov %rsi, %rdx"); // rdx <- rsi
                sb.AppendLine("  mov %rdi, %rcx"); // rcx <- rdi

                sb.AppendLine("  xor %r15, %r15");
                sb.AppendLine("  xor %r14, %r14");
                sb.AppendLine("  xor %r13, %r13");
                sb.AppendLine("  xor %r12, %r12");
                sb.AppendLine("  xor %r11, %r11");
                sb.AppendLine("  xor %r10, %r10");
                sb.AppendLine("  xor %rdi, %rdi");
                sb.AppendLine("  xor %rsi, %rsi");

                if (initInstrs != null) sb.AppendLine(initInstrs);

                sb.AppendLine("\n.balign  64\n" + funcName + "start:");
                if (preLoadInstrs1 != null) sb.AppendLine(preLoadInstrs1);
                if (postLoadInstrs1 != null) sb.AppendLine(postLoadInstrs1);
                int fillerInstrCount = includePtrChasingLoads ? counts[i] - 2 : counts[i];
                for (int fillerIdx = 0, instrIdx = 0; fillerIdx < fillerInstrCount; fillerIdx++)
                {
                    if (fillerFunc1 != null) {
                        fillerFunc1(sb, funcName, fillerIdx);
                    } else {
                        sb.AppendLine(fillerInstrs1[instrIdx]);
                        instrIdx = (instrIdx + 1) % fillerInstrs1.Length;
                    }
                }
                if (preLoadInstrs2 != null) sb.AppendLine(preLoadInstrs2);
                if (lfence) sb.AppendLine("lfence");

                sb.AppendLine("  dec %rcx");
                sb.AppendLine("  jne " + funcName + "start");
                sb.AppendLine("  pop %rdx");
                sb.AppendLine("  pop %rcx");
                sb.AppendLine("  pop %r8");
                sb.AppendLine("  pop %r10");
                sb.AppendLine("  pop %r11");
                sb.AppendLine("  pop %r12");
                sb.AppendLine("  pop %r13");
                sb.AppendLine("  pop %r14");
                sb.AppendLine("  pop %r15");
                sb.AppendLine("  pop %rdi");
                sb.AppendLine("  pop %rsi");
                sb.AppendLine("  ret\n\n");
            }
        }

        public static void GenerateX86AsmStructureTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            string[] fillerInstrs1,
            string[] fillerInstrs2,
            bool includePtrChasingLoads = true,
            string initInstrs = null,
            string postLoadInstrs1 = null,
            string postLoadInstrs2 = null,
            bool lfence = true,
            Action<StringBuilder, string, int> fillerFunc1 = null,
            Action<StringBuilder, string, int> fillerFunc2 = null)
        {

            initInstrs = initInstrs + "  lea 0x0(%rdx), %rdi\n" + "  lea 0x100(%rdx), %rsi\n";

            string preLoadInstrs1 = "  mov (%rdi), %rdi\n  add %rdx, %rdi\n";
            string preLoadInstrs2 = "  mov (%rsi), %rsi\n  add %rdx, %rsi\n";

            GenerateX86AsmThroughputTestFuncs(sb, 
                counts, 
                funcNamePrefix, 
                fillerInstrs1, 
                fillerInstrs2, 
                includePtrChasingLoads, 
                initInstrs, 
                postLoadInstrs1, 
                postLoadInstrs2, 
                preLoadInstrs1, 
                preLoadInstrs2, 
                lfence,
                fillerFunc1,
                fillerFunc2);
        }

        public static void GenerateX86AsmJumpSchedTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            Action<StringBuilder, string, int> fillerFunc)
        {

            GenerateX86AsmStructureTestFuncs(sb,
                counts,
                funcNamePrefix,
                null,
                null,
                false,
                null,
                null,
                null,
                true,
                fillerFunc,
                fillerFunc);
        }


        /// <summary>
        /// Generates pointer chasing test functions in assembly, with xmm0 <- [address using offset from ptr chasing result]
        /// xmm1-4 can be used for
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="counts"></param>
        /// <param name="funcNamePrefix"></param>
        /// <param name="fillerInstrs1"></param>
        /// <param name="fillerInstrs2"></param>
        public static void GenerateX86AsmFpSchedTestFuncs(StringBuilder sb, int[] counts, string funcNamePrefix, string[] fillerInstrs1, string[] fillerInstrs2)
        {
            // initialize some FP values off r8 (third argument)
            string initInstrs = "  movss 16(%rdx), %xmm1\n" +
                "  movss 16(%rdx), %xmm2\n" +
                "  movss 16(%rdx), %xmm3\n" + 
                "  movss 16(%rdx), %xmm4\n" +
                "  movss 16(%rdx), %xmm5\n";
            
            // post-load instructions
            string postLoadInstrs1 = "  movss 16(%rdi), %xmm0";
            string postLoadInstrs2 = "  movss 16(%rsi), %xmm0";

            GenerateX86AsmStructureTestFuncs(sb, counts, funcNamePrefix, fillerInstrs1, fillerInstrs2, false, initInstrs, postLoadInstrs1, postLoadInstrs2, false);
        }

        public static void GenerateX86AsmFp256SchedTestFuncs(StringBuilder sb, int[] counts, string funcNamePrefix, string[] fillerInstrs1, string[] fillerInstrs2)
        {
            string initInstrs = "  vzeroupper\n" + 
                "  vmovups 16(%rdx), %ymm1\n" + 
                "  vmovups 16(%rdx), %ymm2\n" + 
                "  vmovups 16(%rdx), %ymm3\n" +
                "  vmovups 16(%rdx), %ymm4\n" +
                "  vmovups 16(%rdx), %ymm5\n";
            
            // post-load instructions
            string postLoadInstrs1 = "  vbroadcastss 16(%rdi), %ymm0";
            string postLoadInstrs2 = "  vbroadcastss 16(%rsi), %ymm0";

            GenerateX86AsmStructureTestFuncs(sb, counts, funcNamePrefix, fillerInstrs1, fillerInstrs2, true, initInstrs, postLoadInstrs1, postLoadInstrs2, false);
        }
        public static void GenerateX86AsmFp512SchedTestFuncs(StringBuilder sb, int[] counts, string funcNamePrefix, string[] fillerInstrs1, string[] fillerInstrs2)
        {
            string initInstrs = "  vzeroupper\n" + 
                "  vmovups 16(%rdx), %zmm1\n" +
                "  vmovups 16(%rdx), %zmm2\n" +
                "  vmovups 16(%rdx), %zmm3\n" +
                "  vmovups 16(%rdx), %zmm4\n" +
                "  vmovups 16(%rdx), %zmm5\n";
            
            // post-load instructions
            string postLoadInstrs1 = "  vbroadcastss 16(%rdi), %zmm0";
            string postLoadInstrs2 = "  vbroadcastss 16(%rsi), %zmm0";

            GenerateX86AsmStructureTestFuncs(sb, counts, funcNamePrefix, fillerInstrs1, fillerInstrs2, true, initInstrs, postLoadInstrs1, postLoadInstrs2, false);
        }

        /// <summary>
        /// Generates test functions in ARM assembly.
        /// Registers x15-x10 can be used for integer stuff
        /// Args are in x0, x1, x2
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="counts"></param>
        /// <param name="funcNamePrefix"></param>
        /// <param name="fillerInstrs1"></param>
        /// <param name="fillerInstrs2"></param>
        /// <param name="includePtrChasingLoads"></param>
        /// <param name="dsb">use dsb as lfence</param>
        public static void GenerateArmAsmThroughputTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            string[] fillerInstrs1,
            string[] fillerInstrs2,
            bool includePtrChasingLoads = true,
            string initInstrs = null,
            string postLoadInstrs1 = null,
            string postLoadInstrs2 = null,
            string preLoadInstrs1 = null,
            string preLoadInstrs2 = null,
            bool dsb = false,
            Action<StringBuilder, string, int> fillerFunc1 = null,
            Action<StringBuilder, string, int> fillerFunc2 = null)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                string funcName = funcNamePrefix + counts[i];

                // args in x0, x1
                sb.AppendLine("\n" + funcName + ":");
                sb.AppendLine("  sub sp, sp, #0x50");
                sb.AppendLine("  stp x14, x15, [sp, #0x10]");
                sb.AppendLine("  stp x12, x13, [sp, #0x20]");
                sb.AppendLine("  stp x10, x11, [sp, #0x30]");
                sb.AppendLine("  stp x25, x26, [sp, #0x40]");
                sb.AppendLine("  mov x15, 0");
                sb.AppendLine("  mov x14, 0");
                sb.AppendLine("  mov x13, 0");
                sb.AppendLine("  mov x12, 0");
                sb.AppendLine("  mov x11, 0");
                sb.AppendLine("  mov x10, 0");
                sb.AppendLine("  mov x25, 0");
                sb.AppendLine("  mov x26, 0");
                if (initInstrs != null) sb.AppendLine(initInstrs);

                sb.AppendLine("\n.balign  64\n" + funcName + "start:");
                if (preLoadInstrs1 != null) sb.AppendLine(preLoadInstrs1);
                if (postLoadInstrs1 != null) sb.AppendLine(postLoadInstrs1);
                int fillerInstrCount = includePtrChasingLoads ? counts[i] - 2 : counts[i];
                for (int fillerIdx = 0, instrIdx = 0; fillerIdx < fillerInstrCount; fillerIdx++)
                {
                    if (fillerFunc1 != null) {
                        fillerFunc1(sb, funcName, fillerIdx);
                    } else {
                        sb.AppendLine(fillerInstrs1[instrIdx]);
                        instrIdx = (instrIdx + 1) % fillerInstrs1.Length;
                    }
                }

                if (preLoadInstrs2 != null) sb.AppendLine(preLoadInstrs2);
                if (dsb)
                {
                    sb.AppendLine("  dsb sy");
                    sb.AppendLine("  isb sy");
                }

                sb.AppendLine("  sub x0, x0, 1");
                sb.AppendLine("  cbnz x0, " + funcName + "start");
                sb.AppendLine("  ldp x25, x26, [sp, #0x40]");
                sb.AppendLine("  ldp x10, x11, [sp, #0x30]");
                sb.AppendLine("  ldp x12, x13, [sp, #0x20]");
                sb.AppendLine("  ldp x14, x15, [sp, #0x10]");
                sb.AppendLine("  add sp, sp, #0x50");
                sb.AppendLine("  ret\n\n");
            }
        }

        public static void GenerateArmAsmStructureTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            string[] fillerInstrs1,
            string[] fillerInstrs2,
            bool includePtrChasingLoads = false,
            string initInstrs = null,
            string postLoadInstrs1 = null,
            string postLoadInstrs2 = null,
            bool dsb = true,
            Action<StringBuilder, string, int> fillerFunc1 = null,
            Action<StringBuilder, string, int> fillerFunc2 = null)
        {

            initInstrs = initInstrs + "  add x25, x1, 0x0\n" + "  add x26, x1, 0x100";

            string preLoadInstrs1 = "  ldr x25, [x25]\n  add x25, x1, x25";
            string preLoadInstrs2 = "  ldr x26, [x26]\n  add x26, x1, x26";

            GenerateArmAsmThroughputTestFuncs(sb, 
                counts, 
                funcNamePrefix, 
                fillerInstrs1, 
                fillerInstrs2, 
                includePtrChasingLoads, 
                initInstrs, 
                postLoadInstrs1, 
                postLoadInstrs2, 
                preLoadInstrs1, 
                preLoadInstrs2, 
                dsb,
                fillerFunc1,
                fillerFunc2);
        }

        public static void GenerateArmAsmJumpSchedTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            Action<StringBuilder, string, int> fillerFunc = null)
        {

            GenerateArmAsmStructureTestFuncs(sb,
                counts,
                funcNamePrefix,
                null,
                null,
                false,
                null,
                null,
                null,
                true,
                fillerFunc,
                fillerFunc);
        }

        public static void GenerateArmAsmFpSchedTestFuncs(StringBuilder sb, int[] counts, string funcNamePrefix, string[] fillerInstrs1, string[] fillerInstrs2)
        {
            string initInstrs = "  ldr s17, [x1, #16]\n" +
                "ldr s18, [x1, #16]\n" + 
                "ldr s19, [x1, #16]\n" +
                "ldr s20, [x1, #16]\n" +
                "ldr s21, [x1, #16]\n";

            string postLoadInstrs1 = "  ldr s16, [x25, #16]\n";
            string postLoadInstrs2 = "  ldr s16, [x26, #16]\n";

            GenerateArmAsmStructureTestFuncs(sb,
                counts,
                funcNamePrefix,
                fillerInstrs1,
                fillerInstrs2,
                false,
                initInstrs,
                postLoadInstrs1,
                postLoadInstrs2,
                false);
        }

        /// <summary>
        /// Generates test functions in RISCV assembly.
        /// Registers t0-t5, s1-s2 can be used for integer stuff
        /// Args are in a0, a1, a2
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="counts"></param>
        /// <param name="funcNamePrefix"></param>
        /// <param name="fillerInstrs1"></param>
        /// <param name="fillerInstrs2"></param>
        /// <param name="includePtrChasingLoads"></param>
        /// <param name="fence">use fence as lfence</param>
        public static void GenerateRiscvAsmThroughputTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            string[] fillerInstrs1,
            string[] fillerInstrs2,
            bool includePtrChasingLoads = false,
            string initInstrs = null,
            string postLoadInstrs1 = null,
            string postLoadInstrs2 = null,
            string preLoadInstrs1 = null,
            string preLoadInstrs2 = null,
            bool fence = false,
            Action<StringBuilder, string, int> fillerFunc1 = null,
            Action<StringBuilder, string, int> fillerFunc2 = null)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                string funcName = funcNamePrefix + counts[i];

                // args in a0, a1
                sb.AppendLine("\n" + funcName + ":");
                sb.AppendLine("  addi sp, sp, -80");  // Adjust stack (16-byte aligned)
                sb.AppendLine("  sd s2, 8(sp)");
                sb.AppendLine("  sd s1, 16(sp)");
                sb.AppendLine("  sd t6, 24(sp)");
                sb.AppendLine("  sd t5, 32(sp)");
                sb.AppendLine("  sd t4, 40(sp)");
                sb.AppendLine("  sd t3, 48(sp)");
                sb.AppendLine("  sd t2, 56(sp)");
                sb.AppendLine("  sd t1, 64(sp)");
                sb.AppendLine("  sd t0, 72(sp)");

                sb.AppendLine("  li t6, 0");
                sb.AppendLine("  li t5, 0");
                sb.AppendLine("  li t4, 0");
                sb.AppendLine("  li t3, 0");
                sb.AppendLine("  li t2, 0");
                sb.AppendLine("  li t1, 0");
                sb.AppendLine("  li t0, 0");
                sb.AppendLine("  li s2, 0");
                sb.AppendLine("  li s1, 0");

                if (initInstrs != null) sb.AppendLine(initInstrs);

                sb.AppendLine("\n.balign  64\n" + funcName + "start:");
                if (preLoadInstrs1 != null) sb.AppendLine(preLoadInstrs1);
                if (postLoadInstrs1 != null) sb.AppendLine(postLoadInstrs1);
                int fillerInstrCount = includePtrChasingLoads ? counts[i] - 2 : counts[i];
                for (int fillerIdx = 0, instrIdx = 0; fillerIdx < fillerInstrCount; fillerIdx++)
                {
                    if (fillerFunc1 != null) {
                        fillerFunc1(sb, funcName, fillerIdx);
                    } else {
                        sb.AppendLine(fillerInstrs1[instrIdx]);
                        instrIdx = (instrIdx + 1) % fillerInstrs1.Length;
                    }
                }
                if (preLoadInstrs2 != null) sb.AppendLine(preLoadInstrs2);
                if (fence)
                {
                    sb.AppendLine("  fence iorw, iorw");
                    // In gem5, ordinary RISC-V fence is a memory-ordering
                    // barrier, not a dispatch-serializing lfence. fence.i has
                    // SerializeAfter semantics and gives structure tests a loop
                    // boundary closer to the x86 lfence version.
                    sb.AppendLine("  fence.i");
                }

                sb.AppendLine("  addi a0, a0, -1");
                sb.AppendLine("  bnez a0, " + funcName + "start");
                sb.AppendLine("  ld s2, 8(sp)");
                sb.AppendLine("  ld s1, 16(sp)");
                sb.AppendLine("  ld t6, 24(sp)");
                sb.AppendLine("  ld t5, 32(sp)");
                sb.AppendLine("  ld t4, 40(sp)");
                sb.AppendLine("  ld t3, 48(sp)");
                sb.AppendLine("  ld t2, 56(sp)");
                sb.AppendLine("  ld t1, 64(sp)");
                sb.AppendLine("  ld t0, 72(sp)");
                sb.AppendLine("  addi sp, sp, 80");  // Adjust stack (16-byte aligned)
                sb.AppendLine("  ret\n\n");
            }
        }
        public static void GenerateRiscvAsmStructureTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            string[] fillerInstrs1,
            string[] fillerInstrs2,
            bool includePtrChasingLoads = false,
            string initInstrs = null,
            string postLoadInstrs1 = null,
            string postLoadInstrs2 = null,
            bool fence = true,
            Action<StringBuilder, string, int> fillerFunc1 = null,
            Action<StringBuilder, string, int> fillerFunc2 = null)
        {

            initInstrs = initInstrs + "  add s1, a1, 0\n" + "  add s2, a1, 0x100\n";   

            string preLoadInstrs1 = "  ld s1, 0(s1)\n  add s1, a1, s1\n";
            string preLoadInstrs2 = "  ld s2, 0(s2)\n  add s2, a1, s2\n";

            GenerateRiscvAsmThroughputTestFuncs(sb,
                counts,
                funcNamePrefix,
                fillerInstrs1,
                fillerInstrs2,
                includePtrChasingLoads,
                initInstrs,
                postLoadInstrs1,
                postLoadInstrs2,
                preLoadInstrs1,
                preLoadInstrs2,
                fence,
                fillerFunc1,
                fillerFunc2);
        }

        public static string GenerateRiscvDivBlocker(int divCount, string addressDestReg = null)
        {
            StringBuilder blocker = new StringBuilder();
            blocker.AppendLine("  li t0, 1234567");
            blocker.AppendLine("  li t1, 3");
            for (int i = 0; i < divCount; i++)
            {
                blocker.AppendLine("  divu t0, t0, t1");
            }
            if (addressDestReg != null)
            {
                blocker.AppendLine("  andi t0, t0, 0");
                blocker.AppendLine($"  add {addressDestReg}, a1, t0");
            }
            return blocker.ToString();
        }

        public static void GenerateRiscvAsmBlockedStructureTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            string[] fillerInstrs,
            string initInstrs = null,
            int divCount = 4,
            string addressDestReg = null,
            bool fence = true,
            Action<StringBuilder, string, int> fillerFunc = null)
        {
            GenerateRiscvAsmThroughputTestFuncs(sb,
                counts,
                funcNamePrefix,
                fillerInstrs,
                fillerInstrs,
                false,
                initInstrs,
                null,
                null,
                GenerateRiscvDivBlocker(divCount, addressDestReg),
                null,
                fence,
                fillerFunc,
                fillerFunc);
        }

        public static void GenerateRiscvAsmJumpSchedTestFuncs(StringBuilder sb,
            int[] counts,
            string funcNamePrefix,
            Action<StringBuilder, string, int> fillerFunc = null)
        {

            GenerateRiscvAsmStructureTestFuncs(sb,
                counts,
                funcNamePrefix,
                null,
                null,
                false,
                null,
                null,
                null,
                true,
                fillerFunc,
                fillerFunc);
        }

        public static void GenerateRiscvAsmFpSchedTestFuncs(StringBuilder sb, int[] counts, string funcNamePrefix, string[] fillerInstrs1, string[] fillerInstrs2)
        {

            string initInstrs = "  flw f1, 0(a1)\n" +
                "  flw f2, 4(a1)\n" +
                "  flw f3, 8(a1)\n" +
                "  flw f4, 12(a1)\n" +
                "  flw f5, 16(a1)\n";

            string postLoadInstrs1 = "  flw f0, 16(s1)\n";
            string postLoadInstrs2 = "  flw f0, 16(s2)\n";

            GenerateRiscvAsmStructureTestFuncs(sb,
                counts,
                funcNamePrefix,
                fillerInstrs1,
                fillerInstrs2,
                false,
                initInstrs,
                postLoadInstrs1,
                postLoadInstrs2,
                false);
        }
    }
}
