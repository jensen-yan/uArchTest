/* This is a series of micro-tests designed to identify and report the execution rate of various instructions on x86-64 processor.
 * The tests are principally in assembly, that is wrapped in C code to handle timing and reporting results
 */

#include <stdio.h>
#include <sys/time.h>
#include <time.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <cpuid.h>

#define intTestArrLength 1024

extern uint64_t noptest(uint64_t iterations) asm("noptest") __attribute((sysv_abi));
extern uint64_t noptest1b(uint64_t iterations) asm("noptest1b") __attribute((sysv_abi));
extern uint64_t clktest(uint64_t iterations) asm("clktest") __attribute((sysv_abi));
extern uint64_t clkmovtest(uint64_t iterations) asm("clkmovtest") __attribute((sysv_abi));
extern uint64_t addtest(uint64_t iterations) asm("addtest") __attribute((sysv_abi));
extern uint64_t addnoptest(uint64_t iterations) asm("addnoptest") __attribute((sysv_abi));
extern uint64_t addmovtest(uint64_t iterations) asm("addmovtest") __attribute((sysv_abi));
extern uint64_t leatest(uint64_t iterations) asm("leatest") __attribute((sysv_abi));
extern uint64_t leamultest(uint64_t iterations) asm("leamultest") __attribute((sysv_abi));
extern uint64_t rortest(uint64_t iterations) asm("rortest") __attribute((sysv_abi));
extern uint64_t shltest(uint64_t iterations) asm("shltest") __attribute((sysv_abi));
extern uint64_t rorbtstest(uint64_t iterations) asm("rorbtstest") __attribute((sysv_abi));
extern uint64_t mixrormultest(uint64_t iterations) asm("mixrormultest") __attribute((sysv_abi));
extern uint64_t mixrorshltest(uint64_t iterations) asm("mixrorshltest") __attribute((sysv_abi));
extern uint64_t btstest(uint64_t iterations) asm("btstest") __attribute((sysv_abi));
extern uint64_t btsmultest(uint64_t iterations) asm("btsmultest") __attribute((sysv_abi));
extern uint64_t addmultest(uint64_t iterations) asm("addmultest") __attribute((sysv_abi));
extern uint64_t jmpmultest(uint64_t iterations) asm("jmpmultest") __attribute((sysv_abi));
extern uint64_t jmptest(uint64_t iterations) asm("jmptest") __attribute((sysv_abi));
extern uint64_t ntjmptest(uint64_t iterations) asm("ntjmptest") __attribute((sysv_abi));
extern uint64_t mixadd256int(uint64_t iterations) asm("mixadd256int") __attribute((sysv_abi));
extern uint64_t mixadd256int11(uint64_t iterations) asm("mixadd256int11") __attribute((sysv_abi));
extern uint64_t mixadd256fpint(uint64_t iterations) asm("mixadd256fpint") __attribute((sysv_abi));
extern uint64_t mix256fp(uint64_t iterations) asm("mix256fp") __attribute((sysv_abi));
extern uint64_t mix256fp11(uint64_t iterations) asm("mix256fp11") __attribute((sysv_abi));
extern uint64_t latadd512int(uint64_t iterations) asm("latadd512int") __attribute((sysv_abi));
extern uint64_t latadd256int(uint64_t iterations) asm("latadd256int") __attribute((sysv_abi));
extern uint64_t latadd128int(uint64_t iterations) asm("latadd128int") __attribute((sysv_abi));
extern uint64_t latadd256fp(uint64_t iterations) asm("latadd256fp") __attribute((sysv_abi));
extern uint64_t latmul128int(uint64_t iterations) asm("latmul128int") __attribute((sysv_abi));
extern uint64_t latmul256int(uint64_t iterations) asm("latmul256int") __attribute((sysv_abi));
extern uint64_t latmul512int(uint64_t iterations) asm("latmul512int") __attribute((sysv_abi));
extern uint64_t latmulq512int(uint64_t iterations) asm("latmulq512int") __attribute((sysv_abi));
extern uint64_t latmuldq512int(uint64_t iterations) asm("latmuldq512int")__attribute((sysv_abi));
extern uint64_t latmul256fp(uint64_t iterations) asm("latmul256fp") __attribute((sysv_abi));
extern uint64_t latadd128fp(uint64_t iterations) asm("latadd128fp") __attribute((sysv_abi));
extern uint64_t latmul128fp(uint64_t iterations) asm("latmul128fp") __attribute((sysv_abi));
extern uint64_t latfma512(uint64_t iterations) asm("latfma512") __attribute((sysv_abi));
extern uint64_t latfma256(uint64_t iterations) asm("latfma256") __attribute((sysv_abi));
extern uint64_t latfma128(uint64_t iterations) asm("latfma128") __attribute((sysv_abi));
extern uint64_t add128int(uint64_t iterations) asm("add128int") __attribute((sysv_abi));
extern uint64_t add256int(uint64_t iterations) asm("add256int") __attribute((sysv_abi));
extern uint64_t add512int(uint64_t iterations) asm("add512int") __attribute((sysv_abi));
extern uint64_t mul512int(uint64_t iterations) asm("mul512int") __attribute((sysv_abi));
extern uint64_t muldq512int(uint64_t iterations) asm("muldq512int") __attribute((sysv_abi));
extern uint64_t mul128int(uint64_t iterations) asm("mul128int") __attribute((sysv_abi));
extern uint64_t add128fp(uint64_t iterations) asm("add128fp") __attribute((sysv_abi));
extern uint64_t mul128fp(uint64_t iterations) asm("mul128fp") __attribute((sysv_abi));
extern uint64_t fma512(uint64_t iterations) asm("fma512") __attribute((sysv_abi));
extern uint64_t mixfma256fma512(uint64_t iterations) asm("mixfma256fma512") __attribute((sysv_abi));
extern uint64_t mix21fma256fma512(uint64_t iterations) asm("mix21fma256fma512")__attribute((sysv_abi));
extern uint64_t fma256(uint64_t iterations) asm("fma256") __attribute((sysv_abi));
extern uint64_t fma128(uint64_t iterations) asm("fma128") __attribute((sysv_abi));
extern uint64_t mixfmafadd256(uint64_t iterations) asm("mixfmafadd256") __attribute((sysv_abi));
extern uint64_t mixfmaadd256(uint64_t iterations) asm("mixfmaadd256") __attribute((sysv_abi));
extern uint64_t mixfmaadd512(uint64_t iterations) asm("mixfmaadd512") __attribute((sysv_abi));
extern uint64_t mixfma512add256(uint64_t iterations) asm("mixfma512add256") __attribute((sysv_abi));
extern uint64_t mixfmaand256(uint64_t iterations) asm("mixfmaand256") __attribute((sysv_abi));
extern uint64_t mixfmaandmem256(uint64_t iterations, float *arr) asm("mixfmaandmem256") __attribute((sysv_abi));
extern uint64_t mixfmaaddmem256(uint64_t iterations, float *arr) asm("mixfmaaddmem256") __attribute((sysv_abi));
extern uint64_t nemesfpumix21(uint64_t iterations) asm("nemesfpumix21") __attribute((sysv_abi));
extern uint64_t nemesfpu512mix21(uint64_t iterations) asm("nemesfpu512mix21") __attribute((sysv_abi));
extern uint64_t mul256fp(uint64_t iterations) asm("mul256fp") __attribute((sysv_abi));
extern uint64_t add256fp(uint64_t iterations) asm("add256fp") __attribute((sysv_abi));
extern uint64_t latmul64(uint64_t iterations) asm("latmul64") __attribute((sysv_abi));
extern uint64_t latmul16(uint64_t iterations) asm("latmul16") __attribute((sysv_abi));
extern uint64_t mul16(uint64_t iterations) asm("mul16") __attribute((sysv_abi));
extern uint64_t mul64(uint64_t iterations) asm("mul64") __attribute((sysv_abi));
extern uint64_t load128(uint64_t iterations, int *arr) asm("load128") __attribute((sysv_abi));
extern uint64_t spacedload128(uint64_t iterations, int *arr) asm("spacedload128") __attribute((sysv_abi));
extern uint64_t load256(uint64_t iterations, float *arr) asm("load256") __attribute((sysv_abi));
extern uint64_t load512(uint64_t iterations, float *arr) asm("load512") __attribute((sysv_abi));
extern uint64_t store128(uint64_t iterations, int *arr, int *sink) asm("store128") __attribute((sysv_abi));
extern uint64_t store256(uint64_t iterations, float *arr, float *sink) asm("store256") __attribute((sysv_abi));
extern uint64_t store512(uint64_t iterations, float *arr, float *sink) asm("store512") __attribute((sysv_abi));
extern uint64_t mixaddmul128int(uint64_t iterations) asm("mixaddmul128int") __attribute((sysv_abi));
extern uint64_t mixmul16mul64(uint64_t iterations) asm("mixmul16mul64") __attribute((sysv_abi));
extern uint64_t mixmul16mul64_21(uint64_t iterations) asm("mixmul16mul64_21") __attribute((sysv_abi));
extern uint64_t pdeptest(uint64_t iterations) asm("pdeptest") __attribute((sysv_abi));
extern uint64_t pdepmultest(uint64_t iterations) asm("pdepmultest") __attribute((sysv_abi));
extern uint64_t pexttest(uint64_t iterations) asm("pexttest") __attribute((sysv_abi));
extern uint64_t indepmovtest(uint64_t iterations) asm("indepmovtest") __attribute((sysv_abi));
extern uint64_t vecindepmovtest(uint64_t iterations) asm("vecindepmovtest") __attribute((sysv_abi));
extern uint64_t depmovtest(uint64_t iterations) asm("depmovtest") __attribute((sysv_abi));
extern uint64_t vecdepmovtest(uint64_t iterations) asm("vecdepmovtest") __attribute((sysv_abi));
extern uint64_t xorzerotest(uint64_t iterations) asm("xorzerotest") __attribute((sysv_abi));
extern uint64_t vecxorzerotest(uint64_t iterations) asm("vecxorzerotest") __attribute((sysv_abi));
extern uint64_t movzerotest(uint64_t iterations) asm("movzerotest") __attribute((sysv_abi));
extern uint64_t subzerotest(uint64_t iterations) asm("subzerotest") __attribute((sysv_abi));
extern uint64_t vecsubzerotest(uint64_t iterations) asm("vecsubzerotest") __attribute((sysv_abi));
extern uint64_t depinctest(uint64_t iterations) asm("depinctest") __attribute((sysv_abi));
extern uint64_t depdectest(uint64_t iterations) asm("depdectest") __attribute((sysv_abi));
extern uint64_t depaddimmtest(uint64_t iterations) asm("depaddimmtest") __attribute((sysv_abi));
extern uint64_t spacedstorescalar(uint64_t iterations, int *arr) asm("spacedstorescalar") __attribute((sysv_abi));
extern uint64_t aesenc128(uint64_t iterations) asm("aesenc128") __attribute((sysv_abi));
extern uint64_t aesdec128(uint64_t iterations) asm("aesdec128") __attribute((sysv_abi));
extern uint64_t aesencfadd128(uint64_t iterations) asm("aesencfadd128") __attribute((sysv_abi));
extern uint64_t aesencadd128(uint64_t iterations) asm("aesencadd128") __attribute((sysv_abi));
extern uint64_t aesencfma128(uint64_t iterations) asm("aesencfma128") __attribute((sysv_abi));
extern uint64_t aesencmul128(uint64_t iterations) asm("aesencmul128") __attribute((sysv_abi));
extern uint64_t mix256faddintadd(uint64_t iterations) asm("mix256faddintadd") __attribute((sysv_abi));

extern uint64_t fma4_256(uint64_t iterations) asm("fma4_256") __attribute((sysv_abi));
extern uint64_t fma4_128(uint64_t iterations) asm("fma4_128") __attribute((sysv_abi));

extern uint64_t testfusion(uint64_t iterations) asm("testfusion") __attribute((sysv_abi));
extern uint64_t cmpfusion(uint64_t iterations) asm("cmpfusion") __attribute((sysv_abi));
extern uint64_t subfusion(uint64_t iterations) asm("subfusion")__attribute((sysv_abi));
extern uint64_t nopfusion(uint64_t iterations) asm("nopfusion") __attribute((sysv_abi));

float fpTestArr[8] __attribute__ ((aligned (64))) = { 0.2, 1.5, 2.7, 3.14, 5.16, 6.3, 7.7, 9.45 };
float fpSinkArr[8] __attribute__ ((aligned (64))) = { 2.1, 3.2, 4.3, 5.4, 6.2, 7.8, 8.3, 9.4 };
int intTestArr[intTestArrLength] __attribute__ ((aligned (64)));
int intSinkArr[8] __attribute__ ((aligned (64))) = { 2, 3, 4, 5, 6, 7, 8, 9 };

uint64_t load128wrapper(uint64_t iterations) __attribute((sysv_abi));
uint64_t spacedload128wrapper(uint64_t iterations) __attribute((sysv_abi));
uint64_t spacedstorescalarwrapper(uint64_t iterations) __attribute((sysv_abi));
uint64_t load256wrapper(uint64_t iterations) __attribute((sysv_abi));
uint64_t load512wrapper(uint64_t iterations) __attribute((sysv_abi));
uint64_t store128wrapper(uint64_t iterations) __attribute((sysv_abi));
uint64_t store256wrapper(uint64_t iterations) __attribute((sysv_abi));
uint64_t store512wrapper(uint64_t iterations) __attribute((sysv_abi));
uint64_t mixfmaandmem256wrapper(uint64_t iterations)  __attribute((sysv_abi));
uint64_t mixfmaaddmem256wrapper(uint64_t iterations)  __attribute((sysv_abi));

static inline int argumentCheck(int argc, char* argument, char* test, size_t argLength);
float measureFunction(uint64_t iterations, float clockSpeedGhz, __attribute((sysv_abi)) uint64_t (*testfunc)(uint64_t));
