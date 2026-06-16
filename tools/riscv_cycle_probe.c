#include <stdint.h>
#include <stdio.h>

static inline uint64_t
read_cycle(void)
{
    uint64_t value;
    __asm__ volatile("rdcycle %0" : "=r"(value));
    return value;
}

int
main(void)
{
    uint64_t cycle0 = read_cycle();

    volatile uint64_t sink = 0;
    for (uint64_t i = 0; i < 100000; ++i) {
        sink += i;
    }

    uint64_t cycle1 = read_cycle();

    printf("sink=%llu\n", (unsigned long long)sink);
    printf("cycle=%llu,%llu,delta=%llu\n",
           (unsigned long long)cycle0,
           (unsigned long long)cycle1,
           (unsigned long long)(cycle1 - cycle0));

    return 0;
}
