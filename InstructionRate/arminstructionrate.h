#include <stdio.h>
#include <sys/time.h>
#include <time.h>
#include <stdint.h>

extern uint64_t noptest(uint64_t iterations) asm("noptest");
extern uint64_t clktest(uint64_t iterations) asm("clktest");

extern uint64_t addtest(uint64_t iterations) asm("addtest");
extern uint64_t addmultest(uint64_t iterations) asm("addmultest");
extern uint64_t addmul21test(uint64_t iterations) asm("addmul21test");
extern uint64_t mul32test(uint64_t iterations) asm("mul32test");
extern uint64_t mul64test(uint64_t iterations) asm("mul64test");
extern uint64_t latmul64test(uint64_t iterations) asm("latmul64test");
extern uint64_t jmptest(uint64_t iterations) asm("jmptest");
extern uint64_t fusejmptest(uint64_t iterations) asm("fusejmptest");
extern uint64_t mixmuljmptest(uint64_t iterations) asm("mixmuljmptest");
extern uint64_t mixmuljmptest21(uint64_t iterations) asm("mixmuljmptest21");
extern uint64_t mixaddjmptest(uint64_t iterations) asm("mixaddjmptest");
extern uint64_t mixaddjmp21test(uint64_t iterations) asm("mixaddjmp21test");
extern uint64_t rortest(uint64_t iterations) asm("rortest");
extern uint64_t mixmulrortest(uint64_t iterations) asm("mixmulrortest");
extern uint64_t vecadd128test(uint64_t iterations, int arr[4]) asm("vecadd128test");
extern uint64_t latvecadd128test(uint64_t iterations, int arr[4]) asm("latvecadd128test");
extern uint64_t vecmul128test(uint64_t iterations, int arr[4]) asm("vecmul128test");
extern uint64_t latvecmul128test(uint64_t iterations, int arr[4]) asm("latvecmul128test");
extern uint64_t mixvecaddmul128test(uint64_t iterations, int arr[4]) asm("mixvecaddmul128test");
extern uint64_t faddtest(uint64_t iterations, float arr[4]) asm("faddtest");
extern uint64_t latfaddtest(uint64_t iterations, float arr[4]) asm("latfaddtest");
extern uint64_t vecfadd128test(uint64_t iterations, float arr[4]) asm("vecfadd128test");
extern uint64_t vecfmul128test(uint64_t iterations, float arr[4]) asm("vecfmul128test");
extern uint64_t latvecfadd128test(uint64_t iterations, float arr[4]) asm("latvecfadd128test");
extern uint64_t latvecfmul128test(uint64_t iterations, float arr[4]) asm("latvecfmul128test");
extern uint64_t mixvecfaddfmul128test(uint64_t iterations, float arr[4]) asm("mixvecfaddfmul128test");
extern uint64_t vecfma128test(uint64_t iterations, float arr[4]) asm("vecfma128test");
extern uint64_t scalarfmatest(uint64_t iterations, float arr[4]) asm("scalarfmatest");
extern uint64_t latvecfma128test(uint64_t iterations, float arr[4]) asm("latvecfma128test");
extern uint64_t latscalarfmatest(uint64_t iterations, float arr[4]) asm("latscalarfmatest");
extern uint64_t mixvecfaddfma128test(uint64_t iterations, float arr[4]) asm("mixvecfaddfma128test");
extern uint64_t mixvecfmulfma128test(uint64_t iterations, float arr[4]) asm("mixvecfmulfma128test");

// see if SIMD pipeline shares ports with scalar ALU ones
extern uint64_t mixaddvecadd128test(uint64_t iterations, int arr[4]) asm("mixaddvecadd128test");
extern uint64_t mix3to1addvecadd128test(uint64_t iterations, int arr[4]) asm("mix3to1addvecadd128test");
extern uint64_t mix1to1addvecadd128test(uint64_t iterations, int arr[4]) asm("mix1to1addvecadd128test");
extern uint64_t mixmulvecmultest(uint64_t iterations, int arr[4]) asm("mixmulvecmultest");

// are vec int and vec fp on the same port?
extern uint64_t mixvecmulfmultest(uint64_t iterations, float farr[4], int iarr[4]) asm("mixvecmulfmultest");
extern uint64_t mixvecaddfaddtest(uint64_t iterations, float farr[4], int iarr[4]) asm("mixvecaddfaddtest");

// where are the branch ports
extern uint64_t mixjmpvecaddtest(uint64_t iterations, int arr[4]) asm("mixjmpvecaddtest");
extern uint64_t mixjmpvecmultest(uint64_t iterations, int arr[4]) asm("mixjmpvecmultest");

// load/store
extern uint64_t loadtest(uint64_t iterations, int arr[4]) asm("loadtest");
extern uint64_t mixloadstoretest(uint64_t iterations, int arr[4], int sink[4]) asm("mixloadstoretest");
extern uint64_t mix21loadstoretest(uint64_t iterations, int arr[4], int sink[4]) asm("mix21loadstoretest");
extern uint64_t vecloadtest(uint64_t iterations, int arr[4]) asm("vecloadtest");
extern uint64_t vecstoretest(uint64_t iterations, int arr[4], int sink[4]) asm("vecstoretest");

// renamer tests
extern uint64_t indepmovtest(uint64_t iterations) asm("indepmovtest");
extern uint64_t depmovtest(uint64_t iterations) asm("depmovtest");
extern uint64_t xorzerotest(uint64_t iterations) asm("xorzerotest");
extern uint64_t movzerotest(uint64_t iterations) asm("movzerotest");
extern uint64_t subzerotest(uint64_t iterations) asm("subzerotest");


float fpTestArr[4] __attribute__ ((aligned (64))) = { 0.2, 1.5, 2.7, 3.14 };
int intTestArr[4] __attribute__ ((aligned (64))) = { 1, 2, 3, 4 };
int sinkArr[4] __attribute__ ((aligned (64))) = { 2, 3, 4, 5 };

float measureFunction(uint64_t iterations, float clockSpeedGhz, uint64_t (*testfunc)(uint64_t));
uint64_t vecadd128wrapper(uint64_t iterations);
uint64_t latvecadd128wrapper(uint64_t iterations);
uint64_t vecmul128wrapper(uint64_t iterations);
uint64_t latvecmul128wrapper(uint64_t iterations);
uint64_t mixvecaddmul128wrapper(uint64_t iterations);
uint64_t faddwrapper(uint64_t iterations);
uint64_t latfaddwrapper(uint64_t iterations);
uint64_t vecfadd128wrapper(uint64_t iterations);
uint64_t latvecfadd128wrapper(uint64_t iterations);
uint64_t vecfmul128wrapper(uint64_t iterations);
uint64_t latvecfmul128wrapper(uint64_t iterations);
uint64_t mixvecfaddfmul128wrapper(uint64_t iterations);
uint64_t mixaddvecadd128wrapper(uint64_t iterations);
uint64_t mix3to1addvecadd128wrapper(uint64_t iterations);
uint64_t mix1to1addvecadd128wrapper(uint64_t iterations);
uint64_t mixmulvecmulwrapper(uint64_t iterations);
uint64_t mixvecmulfmulwrapper(uint64_t iterations);
uint64_t mixvecaddfaddwrapper(uint64_t iterations);
uint64_t mixjmpvecaddwrapper(uint64_t iterations);
uint64_t mixjmpvecmulwrapper(uint64_t iterations);
uint64_t vecloadwrapper(uint64_t iterations);
uint64_t loadwrapper(uint64_t iterations);
uint64_t vecstorewrapper(uint64_t iterations);
uint64_t mixloadstorewrapper(uint64_t iterations);
uint64_t mix21loadstorewrapper(uint64_t iterations);
uint64_t vecfma128wrapper(uint64_t iterations);
uint64_t scalarfmawrapper(uint64_t iterations);
uint64_t latscalarfmawrapper(uint64_t iterations);
uint64_t mixvecfaddfma128wrapper(uint64_t iterations);
uint64_t mixvecfmulfma128wrapper(uint64_t iterations);
uint64_t latvecfma128wrapper(uint64_t iteration);
