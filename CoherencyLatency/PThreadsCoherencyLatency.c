/*Current goal is to finish adding comments, and then attempt to implement other OS compatibility
*Ideally this would mean only one version of the test is required
*It may not be overly reasonably depending on how different the test works on different operating systems
*Clam did all the hard work, I just cleaned it up a bit
*/


#define _GNU_SOURCE

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <stdint.h>
#include <unistd.h>
#include <sched.h>
#include <pthread.h>

#ifdef __linux__
#include <sys/sysinfo.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/syscall.h>
#endif

#define ITERATIONS 10000000;

// kidding right?
#define gettid() syscall(SYS_gettid)

typedef struct LatencyThreadData 
{
    uint64_t start; //Initial value to write into target
    uint64_t iterations; //number of iterations to run
    uint64_t *target; //value to bounce between threads, init with start - 1
    unsigned int processorIndex;
} LatencyData;

void *LatencyTestThread(void *param);
float RunTest(unsigned int processor1, unsigned int processor2, uint64_t iter);
uint64_t *bouncy;
uint64_t *bouncyBase;

void print_help() {
    fprintf(stderr, "Usage: program_name [options]\n");
    fprintf(stderr, "Options:\n");
    fprintf(stderr, "  -iterations <num>   Set the number of iterations (default: 1)\n");
    fprintf(stderr, "  -bounce             Enable bounce feature\n");
    fprintf(stderr, "  -offset <num>       Set the offset value\n");
    fprintf(stderr, "  -help, --help       Show this help message\n");
}

int main(int argc, char *argv[]) 
{
    float **latencies;
    int numProcs, offsets = 1;
    uint64_t iter = ITERATIONS;

	//sysinfo.h function returns number of available processors.
    numProcs = get_nprocs();
    
    fprintf(stderr, "Number of CPUs: %u\n", numProcs);

	//Ensure that base is memory aligned.  If it cannot be, execution is terminated
    if (0 != posix_memalign((void **)(&bouncyBase), 4096, 4096)) 
	{
        fprintf(stderr, "Could not allocate aligned mem\n");
        return 0;
    }

    for (int argIdx = 1; argIdx < argc; argIdx++) {
        if (*(argv[argIdx]) == '-') {
            char* arg = argv[argIdx] + 1;
            // Parse -help argument
            if (strncmp(arg, "help", 4) == 0) {
                print_help();
                return 0; // Exit after showing help
            }
            // Parse -iterations argument
            else if (strncmp(arg, "iterations", 10) == 0) {
                argIdx++;
                if (argIdx < argc) {
                    iter = atoi(argv[argIdx]);
                    fprintf(stderr, "%lu iterations requested\n", iter);
                } else {
                    fprintf(stderr, "Error: -iterations requires a value\n");
                }
            }
            // Parse -bounce argument
            else if (strncmp(arg, "bounce", 6) == 0) {
                argIdx++;
                // Missing implementation of branch logic
                fprintf(stderr, "Bouncy\n");
            }
            // Parse -offset argument
            else if (strncmp(arg, "offset", 6) == 0) {
                argIdx++;
                if (argIdx < argc) {
                    offsets = atoi(argv[argIdx]);
                    fprintf(stderr, "Offsets: %d\n", offsets);
                } else {
                    fprintf(stderr, "Error: -offset requires a value\n");
                }
            }
            else {
                fprintf(stderr, "Unknown option: %s\n", argv[argIdx]);
                print_help();
                return 1; // Exit with error
            }
        }
    }

	//This block needs reviewing
    latencies = (float **)malloc(sizeof(float *) * offsets);
    memset(latencies, 0, sizeof(float) * offsets);

	//This block needs comments
    for (int offsetIdx = 0; offsetIdx < offsets; offsetIdx++) 
	{
        latencies[offsetIdx] = (float *)malloc(sizeof(float) * numProcs * numProcs);
        float *latenciesPtr = latencies[offsetIdx];
        bouncy = (uint64_t *)((char *)bouncyBase + offsetIdx * 64);

        for (int i = 0;i < numProcs; i++) 
		{
            for (int j = 0;j < numProcs; j++) 
			{
                latenciesPtr[j + i * numProcs] = i == j ? 0 : RunTest(i, j, iter);
            }
        }
    }
	
	//This block needs comments
    for (int offsetIdx = 0; offsetIdx < offsets; offsetIdx++) 
	{
    	float *latenciesPtr = latencies[offsetIdx];
        printf("Cache line offset: %d\n", offsetIdx);
        for (int i = 0;i < numProcs; i++) 
		{
            for (int j = 0;j < numProcs; j++) 
			{
                if (j != 0) printf(",");
                if (j == i) printf("x");
                //dividing by 2 is required on linux.  Other OS's need their own implementation TBA
                else printf("%f", latenciesPtr[j + i * numProcs] / 2);
            }
            printf("\n");
        }
		free(latenciesPtr);
    }

    free(latencies);
    free(bouncyBase);
    return 0;
}

// run test and gather timing data using the specified thread function
float TimeThreads(unsigned int proc1, unsigned int proc2, uint64_t iter, 
					LatencyData *lat1, LatencyData *lat2, void *(*threadFunc)(void *)) 
{
	struct timeval startTv, endTv;
    struct timezone startTz, endTz;
    
	//Threads and the return values for pthread create
	pthread_t testThreads[2];
    int t1rc, t2rc;
    
	//Required for pthread_join.  Value is otherwise unused, could be merged with t1rc/t2rc
	void *res1, *res2;

	//Start Timing
    gettimeofday(&startTv, &startTz);
    t1rc = pthread_create(&testThreads[0], NULL, threadFunc, (void *)lat1);
    t2rc = pthread_create(&testThreads[1], NULL, threadFunc, (void *)lat2);
    
	//Return 0 if thread creation failed (latency is not normally 0 so this is a valid error code)
	if (t1rc != 0 || t2rc != 0) 
	{
      fprintf(stderr, "Could not create threads\n");
      return 0;
	}

    pthread_join(testThreads[0], &res1);
    pthread_join(testThreads[1], &res2);
    gettimeofday(&endTv, &endTz);
	//End Timing
	
    uint64_t time_diff_ms = 1000 * (endTv.tv_sec - startTv.tv_sec) + ((endTv.tv_usec - startTv.tv_usec) / 1000);
    float latency = 1e6 * (float)time_diff_ms / (float)iter;
    return latency;
}

// test latency between two logical CPUs
float RunTest(unsigned int processor1, unsigned int processor2, uint64_t iter) 
{
  LatencyData lat1, lat2;
  float latency;

  *bouncy = 0;
  
  //Initalize Thread 1 Data
  lat1.iterations = iter;
  lat1.start = 1;
  lat1.target = bouncy;
  lat1.processorIndex = processor1;
  
  //Initialize Thread 2 Data
  lat2.iterations = iter;
  lat2.start = 2;
  lat2.target = bouncy;
  lat2.processorIndex = processor2;
  
  latency = TimeThreads(processor1, processor2, iter, &lat1, &lat2, LatencyTestThread);
  fprintf(stderr, "%d to %d: %f ns\n", processor1, processor2, latency);
  return latency;
}

//This function needs reviewing
void *LatencyTestThread(void *param) 
{
    LatencyData *latencyData = (LatencyData *)param;
    cpu_set_t cpuset;
    uint64_t current = latencyData->start;

    CPU_ZERO(&cpuset);
    CPU_SET(latencyData->processorIndex, &cpuset);
    sched_setaffinity(gettid(), sizeof(cpu_set_t), &cpuset);
    //fprintf(stderr, "thread %ld set affinity %d\n", gettid(), latencyData->processorIndex);

    while (current <= 2 * latencyData->iterations) 
	{
        if (__sync_bool_compare_and_swap(latencyData->target, current - 1, current)) 
			current += 2;
    }

    pthread_exit(NULL);
}
