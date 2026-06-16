    .section .text
    .globl preplatencyarr
    .globl latencytest
    .globl stlftest
    .globl stlftest32
    .globl matchedstlftest

/* 
 * x10 = ptr to arr (a0 in RISCV)
 * x11 = arr len (a1 in RISCV)
 * Convert values in array from array indexes to pointers.
 */
preplatencyarr:
    addi sp, sp, -16
    sd t0, 0(sp)
    sd t1, 8(sp)
    li t1, 0               # t1 = index (0)
preplatencyarr_loop:
    slli t0, t1, 3         # t0 = t1 << 3 (array index to byte offset)
    add t0, a0, t0         # t0 = base + offset
    ld t2, 0(t0)           # t2 = arr[t1]
    slli t2, t2, 3         # t2 = t2 << 3
    add t2, t2, a0         # t2 = t2 + base
    sd t2, 0(t0)           # arr[t1] = t2
    addi t1, t1, 1         # index++
    bne t1, a1, preplatencyarr_loop
    ld t0, 0(sp)
    ld t1, 8(sp)
    addi sp, sp, 16
    ret

/*
 * a0 = iteration count
 * a1 = ptr to arr
 * Perform pointer chasing for specified iteration count.
 */
latencytest:
    addi sp, sp, -16
    sd t0, 0(sp)
    sd t1, 8(sp)
    li t0, 0               # t0 = sum
    ld t1, 0(a1)           # t1 = *arr
latencytest_loop:
    ld t1, 0(t1)           # t1 = *t1
    add t0, t0, t1         # sum += t1
    addi a0, a0, -1        # iteration--
    bnez a0, latencytest_loop
    mv a0, t0              # return sum
    ld t0, 0(sp)
    ld t1, 8(sp)
    addi sp, sp, 16
    ret

/*
 * a0 = iteration count
 * a1 = ptr to arr
 * Store-Load Forwarding Test
 * arr[0] = store offset, arr[1] = load offset
 */
stlftest:
    addi sp, sp, -32
    sd t0, 0(sp)
    sd t1, 8(sp)
    sd t2, 16(sp)
    sd t3, 24(sp)
    ld t0, 0(a1)           # t0 = arr[0]
    ld t1, 8(a1)           # t1 = arr[1]
    add t2, a1, t0         # t2 = store pointer
    add t3, a1, t1         # t3 = load pointer
stlftest_loop:
    sd t0, 0(t2)           # store
    ld t0, 0(t3)           # load
    sd t0, 0(t2)           # repeat store
    ld t0, 0(t3)           # repeat load
    sd t0, 0(t2)           # repeat store
    ld t0, 0(t3)           # repeat load
    addi a0, a0, -5        # iteration -= 5
    bgtz a0, stlftest_loop
    ld t0, 0(sp)
    ld t1, 8(sp)
    ld t2, 16(sp)
    ld t3, 24(sp)
    addi sp, sp, 32
    ret

/*
 * Store-Load Forwarding Test for 32-bit access.
 */
stlftest32:
    addi sp, sp, -32
    sd t0, 0(sp)
    sd t1, 8(sp)
    sd t2, 16(sp)
    sd t3, 24(sp)
    ld t0, 0(a1)           # t0 = arr[0]
    ld t1, 8(a1)           # t1 = arr[1]
    add t2, a1, t0         # t2 = store pointer
    add t3, a1, t1         # t3 = load pointer
stlftest32_loop:
    sw t0, 0(t2)           # store 32-bit
    lh t0, 0(t3)           # load half-word
    sw t0, 0(t2)           # repeat store
    lh t0, 0(t3)           # repeat load
    sw t0, 0(t2)           # repeat store
    lh t0, 0(t3)           # repeat load
    addi a0, a0, -5        # iteration -= 5
    bgtz a0, stlftest32_loop
    ld t0, 0(sp)
    ld t1, 8(sp)
    ld t2, 16(sp)
    ld t3, 24(sp)
    addi sp, sp, 32
    ret

/*
 * Matched Store-Load Forwarding Test
 */
matchedstlftest:
    addi sp, sp, -32
    sd t0, 0(sp)
    sd t1, 8(sp)
    sd t2, 16(sp)
    sd t3, 24(sp)
    ld t0, 0(a1)           # t0 = arr[0]
    ld t1, 8(a1)           # t1 = arr[1]
    add t2, a1, t0         # t2 = store pointer
    add t3, a1, t1         # t3 = load pointer
matchedstlftest_loop:
    sd t0, 0(t2)           # store
    ld t0, 0(t3)           # load
    sd t0, 0(t2)           # repeat store
    ld t0, 0(t3)           # repeat load
    sd t0, 0(t2)           # repeat store
    ld t0, 0(t3)           # repeat load
    addi a0, a0, -5        # iteration -= 5
    bgtz a0, matchedstlftest_loop
    ld t0, 0(sp)
    ld t1, 8(sp)
    ld t2, 16(sp)
    ld t3, 24(sp)
    addi sp, sp, 32
    ret

