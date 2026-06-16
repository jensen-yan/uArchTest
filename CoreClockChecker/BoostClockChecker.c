#include <stdio.h>
#include <time.h>
#include <sys/time.h>
#include <stdint.h>
#include <stdlib.h> 
#include <string.h>
#include <unistd.h>

extern uint64_t clktsctest(uint64_t iterations) __attribute((ms_abi));

void print_help() {
    fprintf(stderr, "Usage: program_name [options]\n");
    fprintf(stderr, "Options:\n");
    fprintf(stderr, "  -samples <num>      Set the number of samples\n");
    fprintf(stderr, "  -iterations <num>   Set the number of iterations\n");
    fprintf(stderr, "  -sleep <num>        Set the sleep time in seconds\n");
    fprintf(stderr, "  -help, --help       Show this help message\n");
}

int main(int argc, char *argv[]) {
    struct timeval startTv, endTv;
    uint64_t iterations = 500000, samples = 100;
    unsigned int sleepSeconds = 5;
    time_t time_diff_ms;

    for (int argIdx = 1; argIdx < argc; argIdx++) {
        if (*(argv[argIdx]) == '-') {
            char *arg = argv[argIdx] + 1;

            // Parse -help argument
            if (strncmp(arg, "help", 4) == 0) {
                print_help();
                return 0; // Exit after showing help
            }
            // Parse -samples argument
            else if (strncmp(arg, "samples", 7) == 0) {
                argIdx++;
                if (argIdx < argc) {
                    samples = atol(argv[argIdx]);
                    fprintf(stderr, "Samples set to: %ld\n", samples);
                } else {
                    fprintf(stderr, "Error: -samples requires a value\n");
                    return 1; // Exit with error
                }
            }
            // Parse -iterations argument
            else if (strncmp(arg, "iterations", 10) == 0) {
                argIdx++;
                if (argIdx < argc) {
                    iterations = atol(argv[argIdx]);
                    fprintf(stderr, "Iterations set to: %ld\n", iterations);
                } else {
                    fprintf(stderr, "Error: -iterations requires a value\n");
                    return 1; // Exit with error
                }
            }
            // Parse -sleep argument
            else if (strncmp(arg, "sleep", 5) == 0) {
                argIdx++;
                if (argIdx < argc) {
                    sleepSeconds = atoi(argv[argIdx]);
                    fprintf(stderr, "Sleep time set to: %d seconds\n", sleepSeconds);
                } else {
                    fprintf(stderr, "Error: -sleep requires a value\n");
                    return 1; // Exit with error
                }
            }
            else {
                fprintf(stderr, "Unknown option: %s\n", argv[argIdx]);
                print_help();
                return 1; // Exit with error
            }
        }
    }

    sleep(sleepSeconds);

    uint64_t *measuredTscs = malloc(samples * sizeof(uint64_t));
    for (uint64_t sampleIdx = 0; sampleIdx < samples; sampleIdx++) {
        uint64_t elapsedTsc = clktsctest(iterations);
	measuredTscs[sampleIdx] = elapsedTsc;
    }

    fprintf(stderr, "Used %lu samples\n", samples);
    fprintf(stderr, "Used %lu iterations\n", iterations);
    // figure out TSC to real time ratio
    fprintf(stderr, "Checking TSC ratio...\n");
    uint64_t iterationsHi = 8e9; // should be a couple seconds at least?
    gettimeofday(&startTv, NULL);
    uint64_t referenceElapsedTsc = clktsctest(iterationsHi);
    gettimeofday(&endTv, NULL);
    time_diff_ms = 1000 * (endTv.tv_sec - startTv.tv_sec) + ((endTv.tv_usec - startTv.tv_usec) / 1000);
    float tsc_per_ms = (float)referenceElapsedTsc / (float)time_diff_ms;
    float tsc_per_ns = tsc_per_ms / 1e6;
    fprintf(stderr, "TSC = %lu, elapsed ms = %lu\n", referenceElapsedTsc, time_diff_ms);
    fprintf(stderr, "TSC per ms: %f, TSC per ns: %f\n", tsc_per_ms, tsc_per_ns);

    printf("Time (ms), Clk (GHz), TSC\n");
    float elapsedTime = 0;
    for (uint64_t sampleIdx = 0; sampleIdx < samples; sampleIdx++) {
	// (tsc / ms) * tsc = 1 / ms
	float elapsedTimeMs = measuredTscs[sampleIdx] / tsc_per_ms;
	elapsedTime += elapsedTimeMs;
	float latency = 1e6 * elapsedTimeMs / (float)iterations;
	float addsPerNs = 1 / latency;
	printf("%f,%f,%lu\n", elapsedTime, addsPerNs, measuredTscs[sampleIdx]);
    }

    return 0;
}
