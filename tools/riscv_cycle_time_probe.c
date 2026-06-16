#include <stdint.h>
#include <stdio.h>

static inline uint64_t
read_cycle(void)
{
    uint64_t value;
    __asm__ volatile("rdcycle %0" : "=r"(value));
    return value;
}

static inline uint64_t
read_time(void)
{
    uint64_t value;
    __asm__ volatile("rdtime %0" : "=r"(value));
    return value;
}

int
main(void)
{
    uint64_t cycle0 = read_cycle();
    uint64_t time0 = read_time();

    volatile uint64_t sink = 0;
    for (uint64_t i = 0; i < 100000; ++i) {
        sink += i;
    }

    uint64_t cycle1 = read_cycle();
    uint64_t time1 = read_time();

    printf("sink=%llu\n", (unsigned long long)sink);
    printf("cycle=%llu,%llu,delta=%llu\n",
           (unsigned long long)cycle0,
           (unsigned long long)cycle1,
           (unsigned long long)(cycle1 - cycle0));
    printf("time=%llu,%llu,delta=%llu\n",
           (unsigned long long)time0,
           (unsigned long long)time1,
           (unsigned long long)(time1 - time0));

    return 0;
}
