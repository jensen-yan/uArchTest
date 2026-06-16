using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AsmGen
{
    public class IndirectBranchTest : IUarchTestParallelBuild
    {
        public string Prefix { get; private set; }

        public string Description { get; private set; }

        public string FunctionDefinitionParameters { get; private set; }

        public string GetFunctionCallParameters { get; private set; }

        public bool DivideTimeByCount { get; private set; }

        private int[] branchCounts;
        private int[] targetCounts;
        private int globalHistoryAssistBits;

        public IndirectBranchTest()
        {
            Prefix = "indirectbranch";
            Description = "Indirect branch prediction";
            FunctionDefinitionParameters = "uint64_t iterations, uint32_t **arr, uint32_t arrLen, uint64_t **scratch";
            DivideTimeByCount = true;
            branchCounts = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 };
            targetCounts = new int[] { 2, 4, 8, 12, 16, 24, 32, 48, 64, 96, 128 };
            globalHistoryAssistBits = 4;
        }

        private string GetFunctionName(int branchCount, int targetCount)
        {
            return Prefix + branchCount + "targets" + targetCount;
        }

        private string GetTargetLabelName(int branchCount, int targetCount, int branchIndex, int targetIndex)
        {
            return GetFunctionName(branchCount, targetCount) + "branch" + branchIndex + "target" + targetIndex;
        }

        public void GenerateRiscvAsm(StringBuilder sb)
        {
            for (int targetCountIdx = 0; targetCountIdx < targetCounts.Length; targetCountIdx++) {
                int currentTargetCount = targetCounts[targetCountIdx];
                for (int branchCountIdx = 0; branchCountIdx < branchCounts.Length; branchCountIdx++)
                {
                    int currentBranchCount = branchCounts[branchCountIdx];
                    string functionLabel = GetFunctionName(currentBranchCount, currentTargetCount);
                    string loopLabel = functionLabel + "_loop";

                    sb.AppendLine("\n" + functionLabel + ":");
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

                    // fill in jump tables for every branch. there has to be a better way to do this
                    for (int branchIdx = 0; branchIdx < currentBranchCount; branchIdx++)
                    {
                        // a3 = array of ptrs to jump tables
                        // t3 = index into array of jump tables (branch index)
                        // t6 = ptr to jump table
                        sb.AppendLine("  slli t5, t3, 3");                 // t5 = w14 << 3
                        sb.AppendLine("  add  t5, a3, t5");                // a3 + (w14<<3)
                        sb.AppendLine("  ld   t6, 0(t5)");                 // t6 = *(a3 + w14*8)

                        sb.AppendLine("  li t5, 0");
                        for (int targetIdx = 0; targetIdx < currentTargetCount; targetIdx++)
                        {
                            // assuming 64-bit pointers
                            // use t5 = label address temp, t1 = target label addr
                            string targetLabelName = GetTargetLabelName(currentBranchCount, currentTargetCount, branchIdx, targetIdx);

                            // x10 <- address(label)  ==>  RISC-V: la t1, label
                            sb.AppendLine($"  la   t1, {targetLabelName}");

                            // str x10, [x17, w16, uxtw #3]
                            sb.AppendLine("  slli t4, t5, 3");             // t4 = w16 << 3
                            sb.AppendLine("  add  t4, t6, t4");            // base + scaled index
                            sb.AppendLine("  sd   t1, 0(t4)");             // store pointer

                            // add w16, w16, 1
                            sb.AppendLine("  addi t5, t5, 1");             // w16++
                        }

                        // eor x16, x16, x16   ==>  xor t5,t5,t5 (or mv t5,x0)
                        sb.AppendLine("  xor  t5, t5, t5");                // w16 = 0

                        // add w14, w14, 1
                        sb.AppendLine("  addi t3, t3, 1");                 // w14++
                    }

                    // w14 = branch index (t3), w16 = pattern (target) array index (t5)
                    sb.AppendLine(loopLabel + ":");
                    // eor w14, w14, w14
                    sb.AppendLine("  xor  t3, t3, t3");                    // w14 = 0

                    // generate branch blocks
                    for (int branchIdx = 0; branchIdx < currentBranchCount; branchIdx++)
                    {
                        // get a pointer to the jump table: ldr x9, [x3, w14, uxtw #3]
                        sb.AppendLine("  slli t4, t3, 3");                 // t4 = w14 << 3
                        sb.AppendLine("  add  t4, a3, t4");                // a3 + (w14<<3)
                        sb.AppendLine("  ld   t0, 0(t4)");                 // t0 = x9

                        // look up which target to jump to
                        // ldr x15, [x1, w14, uxtw #3]
                        sb.AppendLine("  slli t4, t3, 3");                 // t4 = w14 << 3
                        sb.AppendLine("  add  t4, a1, t4");                // a1 + (w14<<3)
                        sb.AppendLine("  ld   t4, 0(t4)");                 // t4 = x15 (ptr to pattern array)

                        // add w14, w14, 1
                        sb.AppendLine("  addi t3, t3, 1");                 // w14++

                        // ldr w13, [x15, w16, uxtw #2]  =>  lw t2, 0(t4 + (w16<<2))
                        sb.AppendLine("  slli t1, t5, 2");                 // t1 = w16 << 2
                        sb.AppendLine("  add  t1, t4, t1");                // addr = x15 + (w16<<2)
                        sb.AppendLine("  lw   t2, 0(t1)");                 // t2 = w13 (zero-extended in RV64)

                        // use w13 to index into jump table, and branch on it
                        // ldr x17, [x9, w13, uxtw #3]
                        sb.AppendLine("  slli t1, t2, 3");                 // t1 = w13 << 3
                        sb.AppendLine("  add  t1, t0, t1");                // addr = x9 + (w13<<3)
                        sb.AppendLine("  ld   t6, 0(t1)");                 // t6 = x17 (target addr)

                        // global history assist branches
                        // x18=1  =>  li s1,1
                        sb.AppendLine("  li   s1, 1");
                        // w12=0  =>  xor s2,s2,s2
                        sb.AppendLine("  xor  s2, s2, s2");
                        for (int eaxBits = 0; eaxBits < globalHistoryAssistBits; eaxBits++)
                        {
                            string targetName = functionLabel + "branch" + branchIdx + "ghist" + eaxBits;
                            // and w12, w13, w18
                            sb.AppendLine("  and  s2, t2, s1");
                            // cbnz w12, targetName
                            sb.AppendLine($"  bnez s2, {targetName}");
                            sb.AppendLine("  nop");
                            sb.AppendLine($"{targetName}:");
                            // lsl w18, w18, 1
                            sb.AppendLine("  slli s1, s1, 1");
                        }

                        // br x17  =>  jalr x0, 0(t6)
                        sb.AppendLine("  jalr x0, 0(t6)");
                        sb.AppendLine("  nop");

                        // generate targets
                        for (int targetIdx = 0; targetIdx < currentTargetCount; targetIdx++)
                        {
                            sb.AppendLine(GetTargetLabelName(currentBranchCount, currentTargetCount, branchIdx, targetIdx) + ":");
                            sb.AppendLine("  nop");
                        }
                    }

                    // increment w16, and cmov 0 -> w16 if w16 == w2
                    sb.AppendLine("  addi t5, t5, 1        # t5++");
                    sb.AppendLine("  xor  t1, t5, a2       # t1 = t5 ^ a2");
                    sb.AppendLine("  seqz t1, t1           # t1 = 1 if t5==a2 else 0");
                    sb.AppendLine("  sub  t1, x0, t1       # t1 = -1 if t5==a2 else 0");
                    sb.AppendLine("  xori t1, t1, -1       # t1 = ~t1 → 0 if t5==a2 else -1");
                    sb.AppendLine("  and  t5, t5, t1       # t5 = t5 & t1 → t5=0 if t5==a2 else t5 unchanged");

                    // sub x0, x0, 1
                    sb.AppendLine("  addi a0, a0, -1");
                    // cbnz x0, loopLabel
                    sb.AppendLine($"  bnez a0, {loopLabel}");
                    // mov x0, x12
                    sb.AppendLine("  mv   a0, s0");

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
        }

        public void GenerateArmAsm(StringBuilder sb)
        {
            for (int targetCountIdx = 0; targetCountIdx < targetCounts.Length; targetCountIdx++) {
                int currentTargetCount = targetCounts[targetCountIdx];
                for (int branchCountIdx = 0; branchCountIdx < branchCounts.Length; branchCountIdx++)
                {
                    int currentBranchCount = branchCounts[branchCountIdx];
                    string functionLabel = GetFunctionName(currentBranchCount, currentTargetCount);
                    string loopLabel = functionLabel + "_loop";
                    sb.AppendLine("\n" + functionLabel + ":");
                    sb.AppendLine("  sub sp, sp, #0x60");
                    sb.AppendLine("  stp x17, x18, [sp, #0x50]");
                    sb.AppendLine("  stp x15, x16, [sp, #0x40]");
                    sb.AppendLine("  stp x13, x14, [sp, #0x30]");
                    sb.AppendLine("  stp x11, x12, [sp, #0x20]");
                    sb.AppendLine("  stp x9, x10, [sp, #0x10]");
                    sb.AppendLine("  eor x16, x16, x16");
                    sb.AppendLine("  eor x15, x15, x15");
                    sb.AppendLine("  eor x14, x14, x14");
                    sb.AppendLine("  eor x12, x12, x12");
                    sb.AppendLine("  eor x11, x11, x11");

                    // fill in jump tables for every branch. there has to be a better way to do this
                    for (int branchIdx = 0; branchIdx < currentBranchCount; branchIdx++)
                    {
                        // x3 = array of ptrs to jump tables
                        // x14 = index into array of jump tables
                        // x17 = ptr to jump table
                        sb.AppendLine("  ldr x17, [x3, w14, uxtw #3]");
                        for (int targetIdx = 0; targetIdx < currentTargetCount; targetIdx++)
                        {
                            // assuming 64-bit pointers and 4K page size
                            // use x16 = label index
                            string targetLabelName = GetTargetLabelName(currentBranchCount, currentTargetCount, branchIdx, targetIdx);
                            sb.AppendLine($"  adrp x10, {targetLabelName}");
                            sb.AppendLine($"  add x10, x10, :lo12:{targetLabelName}");
                            sb.AppendLine("  str x10, [x17, w16, uxtw #3]");
                            sb.AppendLine("  add w16, w16, 1");
                        }

                        sb.AppendLine("  eor x16, x16, x16");
                        sb.AppendLine("  add w14, w14, 1");
                    }

                    // w14 = branch index, w16 = pattern (target) array index
                    sb.AppendLine(loopLabel + ":");
                    sb.AppendLine("  eor w14, w14, w14");

                    // generate branch blocks
                    for (int branchIdx = 0; branchIdx < currentBranchCount; branchIdx++)
                    {
                        // get a pointer to the jump table
                        sb.AppendLine("  ldr x9, [x3, w14, uxtw #3]");

                        // look up which target to jump to
                        sb.AppendLine("  ldr x15, [x1, w14, uxtw #3]");
                        sb.AppendLine("  add w14, w14, 1");
                        sb.AppendLine("  ldr w13, [x15, w16, uxtw #2]");

                        // use the target index (w13) to index into the jump table, and branch on it
                        sb.AppendLine("  ldr x17, [x9, w13, uxtw #3]");

                        // global history assist branches
                        // rax = index into jump table. make that correlate with global history
                        sb.AppendLine("  mov x18, 1");
                        sb.AppendLine("  eor w12, w12, w12");
                        for (int eaxBits = 0; eaxBits < globalHistoryAssistBits; eaxBits++)
                        {
                            string targetName = functionLabel + "branch" + branchIdx + "ghist" + eaxBits;
                            sb.AppendLine("  and w12, w13, w18");
                            sb.AppendLine($"  cbnz w12, {targetName}");
                            sb.AppendLine("  nop");
                            sb.AppendLine($"{targetName}:");
                            sb.AppendLine("  lsl w18, w18, 1");
                        }

                        // branch on value of x17
                        sb.AppendLine($"  br x17");
                        sb.AppendLine("  nop");

                        // generate targets
                        for (int targetIdx = 0; targetIdx < currentTargetCount; targetIdx++)
                        {
                            sb.AppendLine(GetTargetLabelName(currentBranchCount, currentTargetCount, branchIdx, targetIdx) + ":");
                            sb.AppendLine($"  nop");
                        }
                    }

                    // increment w16, and basically cmov 0 -> w16 if w16 = list length
                    sb.AppendLine("  add w16, w16, 1");
                    sb.AppendLine("  cmp w16, w2");
                    sb.AppendLine("  csel w16, w11, w16, EQ");
                    sb.AppendLine("  sub x0, x0, 1");
                    sb.AppendLine($"  cbnz x0, {loopLabel}");
                    sb.AppendLine("  mov x0, x12");
                    sb.AppendLine("  ldp x9, x10, [sp, #0x10]");
                    sb.AppendLine("  ldp x11, x12, [sp, #0x20]");
                    sb.AppendLine("  ldp x13, x14, [sp, #0x30]");
                    sb.AppendLine("  ldp x15, x16, [sp, #0x40]");
                    sb.AppendLine("  ldp x17, x18, [sp, #0x50]");
                    sb.AppendLine("  add sp, sp, #0x60");
                    sb.AppendLine("  ret");
                }
            }
        }

        public void GenerateX86GccAsm(StringBuilder sb)
        {
            for (int targetCountIdx = 0; targetCountIdx < targetCounts.Length; targetCountIdx++)
            {
                int currentTargetCount = targetCounts[targetCountIdx];
                for (int branchCountIdx = 0; branchCountIdx < branchCounts.Length; branchCountIdx++)
                {
                    /* rdi = iteration count
                     * rsi = array of target selection arrays, one for each branch
                     * rdx = length of pattern array
                     * rcx = array of jump tables, one for each branch
                     */
                    int currentBranchCount = branchCounts[branchCountIdx];
                    string functionLabel = GetFunctionName(currentBranchCount, currentTargetCount);
                    sb.AppendLine("\n" + functionLabel + ":");
                    sb.AppendLine("  push %rbx");
                    sb.AppendLine("  push %r8");
                    sb.AppendLine("  push %r9");
                    sb.AppendLine("  push %r13");
                    sb.AppendLine("  push %r15");
                    sb.AppendLine("  push %r14");
                    sb.AppendLine("  xor %rbx, %rbx");
                    sb.AppendLine("  xor %r8, %r8");
                    sb.AppendLine("  xor %r9, %r9");

                    // initialize jump table
                    for (int branchIdx = 0; branchIdx < currentBranchCount; branchIdx++)
                    {
                        // rcx = array of ptrs to jump tables
                        // r9 = index into array of jump tables
                        // r15 = ptr to jump table

                        // load jump table base address into r15
                        sb.AppendLine("  mov (%rcx,%r9,8), %r15");
                        for (int targetIdx = 0; targetIdx < currentTargetCount; targetIdx++)
                        {
                            // assuming 64-bit pointers and 4K page size
                            // use rbx = index into
                            string targetLabelName = GetTargetLabelName(currentBranchCount, currentTargetCount, branchIdx, targetIdx);
                            sb.AppendLine($"  lea {targetLabelName}(%rip), %rax");
                            sb.AppendLine($"  mov %rax, (%r15,%rbx,8)");
                            sb.AppendLine("  inc %rbx");
                        }

                        sb.AppendLine("  xor %rbx, %rbx");
                        sb.AppendLine("  inc %r9");
                    }

                    sb.AppendLine("  xor %r8, %r8");
                    sb.AppendLine("  xor %r9, %r9");

                    string loopLabel = functionLabel + "_loop";
                    sb.AppendLine("\n" + loopLabel + ":");
                    sb.AppendLine("  xor %r11, %r11"); // set index into arr of arrs to 0
                    for (int branchIdx = 0; branchIdx < currentBranchCount; branchIdx++)
                    {
                        sb.AppendLine("  mov (%rcx,%r11,8), %r15");  // load jump table base pointer into r15
                        sb.AppendLine("  mov (%rsi,%r11,8), %r10");  // load target select array base pointer into r10
                        sb.AppendLine("  inc %r11");
                        sb.AppendLine("  mov (%r10,%rbx,4), %eax"); // get the target for the current iteration into eax
                        sb.AppendLine("  mov (%r15,%rax,8), %r14");  // load address of jump target from jump table

                        sb.AppendLine("  mov %rsi, %r13");
                        sb.AppendLine("  mov $1, %rsi");
                        for (int eaxBits = 0; eaxBits < 7; eaxBits++)
                        {
                            string targetName = functionLabel + "branch" + branchIdx + "ghist" + eaxBits;
                            sb.AppendLine("  test %eax, %esi");
                            sb.AppendLine($"  jnz {targetName}");
                            sb.AppendLine("  nop");
                            sb.AppendLine($"{targetName}:");
                            sb.AppendLine("  shl $1, %esi");
                        }

                        sb.AppendLine("  mov %r13, %rsi");

                        sb.AppendLine("  jmp *%r14");                // and jump to it
                        // generate targets
                        for (int targetIdx = 0; targetIdx < currentTargetCount; targetIdx++)
                        {
                            sb.AppendLine(GetTargetLabelName(currentBranchCount, currentTargetCount, branchIdx, targetIdx) + ":");
                            sb.AppendLine($"  nop");
                        }
                    }

                    // loop around in pattern history test array if necessary
                    // avoiding an extra branch to not pollute BPU history
                    sb.AppendLine("  inc %rbx");
                    sb.AppendLine("  cmp %rbx, %rdx");
                    sb.AppendLine("  cmove %r9, %rbx");

                    // end of main loop over iteration count
                    sb.AppendLine("  dec %rdi");
                    sb.AppendLine("  jnz " + loopLabel);

                    // function epilogue
                    sb.AppendLine("  mov %r8, %rax");
                    sb.AppendLine("  pop %r14");
                    sb.AppendLine("  pop %r15");
                    sb.AppendLine("  pop %r13");
                    sb.AppendLine("  pop %r9");
                    sb.AppendLine("  pop %r8");
                    sb.AppendLine("  pop %rbx");
                    sb.AppendLine("  ret");
                }
            }
        }

        public void GenerateAsmGlobalLines(StringBuilder sb)
        {
            for (int branchCountIdx = 0; branchCountIdx < branchCounts.Length; branchCountIdx++)
                for (int targetCountIdx = 0; targetCountIdx < targetCounts.Length; targetCountIdx++)
                    sb.AppendLine(".global " + GetFunctionName(branchCounts[branchCountIdx], targetCounts[targetCountIdx]));
        }

        // kinda hack this to put in initialization code we need
        public void GenerateExternLines(StringBuilder sb)
        {
            for (int branchCountIdx = 0; branchCountIdx < branchCounts.Length; branchCountIdx++)
                for (int targetCountIdx = 0; targetCountIdx < targetCounts.Length; targetCountIdx++)
                    sb.AppendLine("extern uint64_t " + GetFunctionName(branchCounts[branchCountIdx], targetCounts[targetCountIdx]) + $"({FunctionDefinitionParameters}) __attribute((sysv_abi));");

            GenerateInitializationCode(sb);
            string gccFunction = File.ReadAllText($"{Program.DataFilesDir}/GccIndirectBranchFunction.c");
            sb.AppendLine(gccFunction);
        }

        public void GenerateInitializationCode(StringBuilder sb)
        {
            sb.AppendLine($"uint32_t maxIndirectBranchCount = {branchCounts.Length};");
            sb.Append($"uint32_t indirectBranchCounts[{branchCounts.Length}] = ");
            sb.Append("{  " + branchCounts[0]);
            for (int i = 1; i < branchCounts.Length; i++) sb.Append(", " + branchCounts[i]);
            sb.AppendLine(" };");
            sb.Append($"uint32_t indirectBranchTargetCounts[{targetCounts.Length}] = ");
            sb.Append("{  " + targetCounts[0]);
            for (int i = 1; i < targetCounts.Length; i++) sb.Append(", " + targetCounts[i]);
            sb.AppendLine(" };");

            // TODO: need to make this a 2D array - [branch count][target count]
            sb.AppendLine($"uint64_t (__attribute((sysv_abi)) *indirectBranchTestFuncArr[{branchCounts.Length}][{targetCounts.Length}])({FunctionDefinitionParameters});");

            sb.AppendLine("void initializeIndirectBranchFuncArr() {");
            for (int i = 0; i < branchCounts.Length; i++)
            {
                for (int targetCountIdx = 0; targetCountIdx < targetCounts.Length; targetCountIdx++)
                {
                    sb.AppendLine($"  indirectBranchTestFuncArr[{i}][{targetCountIdx}] = {GetFunctionName(branchCounts[i], targetCounts[targetCountIdx])};");
                }
            }

            sb.AppendLine("}");
        }

        public void GenerateTestBlock(StringBuilder sb)
        {
            sb.AppendLine("  if (argc > 1 && strncmp(argv[1], \"" + Prefix + "\", " + Prefix.Length + ") == 0) {");
            GenerateCommonTestBlock(sb);
            sb.AppendLine("  }\n");
        }

        public void GenerateCommonTestBlock(StringBuilder sb)
        {
            sb.AppendLine("    printf(\"" + Description + "(\" TIME_UNIT \"):\\n\");");
            string branchhistMain = File.ReadAllText($"{Program.DataFilesDir}/IndirectBranchTestBlock.c");
            sb.AppendLine(branchhistMain);
        }
    }
}
