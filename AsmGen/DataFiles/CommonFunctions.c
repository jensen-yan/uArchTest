// this is a partial C file that's appended into generated code
// stuff here is generic enough to work for both windows/vs and gcc

// Timing configuration - use cycle-level or nanosecond-level timing
// Define USE_CYCLE_TIMING to use cycle-level timers instead of clock_gettime

// Only include time headers when not using cycle timing
#ifndef USE_CYCLE_TIMING
#include <sys/time.h>
#include <time.h>
#endif

#ifdef USE_CYCLE_TIMING

// Cycle-level timing functions for different ISAs
static inline uint64_t read_cycles() {
#if defined(__riscv)
    // RISC-V cycle counter (rdcycle instruction)
    uint64_t cycles;
    __asm__ volatile ("rdcycle %0" : "=r" (cycles));
    return cycles;
#elif defined(__aarch64__)
    // ARM64 cycle counter (PMCCNTR_EL0)
    uint64_t cycles;
    __asm__ volatile ("mrs %0, PMCCNTR_EL0" : "=r" (cycles));
    return cycles;
#elif defined(__x86_64__)
    // x86-64 time stamp counter (rdtsc instruction)
    uint32_t lo, hi;
    __asm__ volatile ("rdtsc" : "=a" (lo), "=d" (hi));
    return ((uint64_t)hi << 32) | lo;
#else
    #error "Unsupported architecture for cycle timing"
#endif
}

#define GET_TIME_START() uint64_t __timing_start = read_cycles()
#define GET_TIME_END() uint64_t __timing_end = read_cycles()
#define CALC_TIME_DIFF() uint64_t __timing_diff = __timing_end - __timing_start
#define TIME_UNIT "cycles"
#define TIME_DIFF_VALUE __timing_diff

#else

// Nanosecond-level timing using clock_gettime (default)
#define GET_TIME_START() struct timespec __timing_start_ts; clock_gettime(CLOCK_MONOTONIC, &__timing_start_ts)
#define GET_TIME_END() struct timespec __timing_end_ts; clock_gettime(CLOCK_MONOTONIC, &__timing_end_ts)
#define CALC_TIME_DIFF() uint64_t __timing_diff = (__timing_end_ts.tv_sec - __timing_start_ts.tv_sec) * 1000000000L + (__timing_end_ts.tv_nsec - __timing_start_ts.tv_nsec)
#define TIME_UNIT "ns"
#define TIME_DIFF_VALUE __timing_diff

#endif

// Seed random number generator - use cycle counter when available, otherwise use time
static inline void seed_random() {
#ifdef USE_CYCLE_TIMING
    srand((unsigned int)read_cycles());
#else
    srand(time(NULL));
#endif
}

void printCsvHeader(uint32_t* xCounts, uint32_t xLen) {
    printf("x");
    for (uint32_t testSizeIdx = 0; testSizeIdx < xLen; testSizeIdx++) {
        printf(", %d", xCounts[testSizeIdx]);
    }

    printf("\n");
}

// print results in format that excel can take
void printResultFloatArr(float* arr, uint32_t *xCounts, uint32_t xLen, uint32_t *yCounts, uint32_t yLen) {
    uint32_t testSizeCount = xLen;
    printCsvHeader(xCounts, xLen);
    for (uint32_t branchCountIdx = 0; branchCountIdx < yLen; branchCountIdx++) {
        // row header
        printf("%d", yCounts[branchCountIdx]);
        for (uint32_t testSizeIdx = 0; testSizeIdx < testSizeCount; testSizeIdx++) {
            printf(",%f", arr[branchCountIdx * testSizeCount + testSizeIdx]);
        }

        printf("\n");
    }
}
