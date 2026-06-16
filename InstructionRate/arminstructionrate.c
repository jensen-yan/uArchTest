/* This is a series of ARM ISA micro-tests designed to identify and report the execution rate of various instructions on x86-64 processor.
 * The tests are principally in assembly, that is wrapped in C code to handle timing and reporting results
 * Clam did all the hard work, Nintonito just cleaned it up a bit.  Give him the real credit
 */

#include "arminstructionrate.h"

int main(int argc, char *argv[]) 
{
  //Base value for iterations.  Latency tests require a separate, higher value
  uint64_t iterations = 1500000000;
  uint64_t latencyIterations = iterations * 5;
  
  float clkSpeed;//CPU Frequency

  //Establish baseline clock speed for CPU, for all further calculations
  clkSpeed = measureFunction(latencyIterations, clkSpeed, clktest);
  printf("Estimated clock speed: %.2f GHz\n", clkSpeed);
  
  

  printf("Adds per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, addtest));
  printf("Nops per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, noptest));
  printf("Indepdent movs per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, indepmovtest));
  printf("Dependent movs per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, depmovtest));
  printf("eor -> 0 per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, xorzerotest));
  printf("mov -> 0 per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, movzerotest));
  printf("sub -> 0 per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, subzerotest));


  printf("Not taken jmps per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, jmptest));
  printf("Jump fusion test: %.2f\n", measureFunction(latencyIterations, clkSpeed, fusejmptest));
  printf("1:1 mixed not taken jmps / muls per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixmuljmptest));
  printf("1:2 mixed not taken jmps / muls per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixmuljmptest21));
  printf("1:1 mixed not taken jmps / adds per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixaddjmptest));
  printf("1:2 mixed not taken jmps / adds per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixaddjmp21test));
  printf("1:1 mixed add/mul per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, addmultest));
  printf("2:1 mixed add/mul per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, addmul21test));
  printf("ror per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, rortest));
  printf("1:1 mixed mul/ror per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixmulrortest));
  printf("32-bit mul per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mul32test));
  printf("64-bit mul per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mul32test));
  printf("scalar fp32 add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, faddwrapper));
  printf("128-bit vec int32 add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecadd128wrapper));
  printf("128-bit vec int32 multiply per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecmul128wrapper));
  printf("128-bit vec int32 mixed multiply and add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixvecaddmul128wrapper));
  printf("128-bit vec fp32 add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecfadd128wrapper));
  printf("128-bit vec fp32 multiply per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecfmul128wrapper));
  printf("128-bit vec fp32 mixed multiply and add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixvecfaddfmul128wrapper));
  printf("2:1 mixed scalar adds and 128-bit vec int32 add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixaddvecadd128wrapper));
  printf("3:1 mixed scalar adds and 128-bit vec int32 add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mix3to1addvecadd128wrapper));
  printf("1:1 mixed scalar adds and 128-bit vec int32 add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mix1to1addvecadd128wrapper));
  printf("1:1 mixed scalar 32-bit multiply and 128-bit vec int32 multiply per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixmulvecmulwrapper));
  printf("1:1 mixed 128-bit vec fp32 multiply and 128-bit vec int32 multiply per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixvecmulfmulwrapper));
  printf("1:1 mixed 128-bit vec fp32 add and 128-bit vec int32 add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixvecaddfaddwrapper));
  printf("1:2 mixed not taken jumps and 128-bit vec int32 add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixjmpvecaddwrapper));
  printf("1:1 mixed not taken jumps and 128-bit vec int32 mul per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixjmpvecmulwrapper));
  printf("128-bit vec loads per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecloadwrapper));
  printf("128-bit vec stores per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecstorewrapper));
  printf("64-bit loads per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, loadwrapper));
  printf("1:1 mixed 64-bit loads/stores per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixloadstorewrapper));
  printf("2:1 mixed 64-bit loads/stores per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mix21loadstorewrapper));
  printf("64-bit multiply latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latmul64test));
  printf("128-bit vec int32 add latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latvecadd128wrapper));
  printf("128-bit vec int32 mul latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latvecmul128wrapper));
  printf("Scalar FADD Latency: %.2f clocks\n", 1 / measureFunction(latencyIterations, clkSpeed, latfaddwrapper));
  printf("128-bit vector FADD latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latvecfadd128wrapper));
  printf("128-bit vector FMUL latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latvecfmul128wrapper));

  printf("128-bit vector FMA per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecfma128wrapper));
  printf("128-bit vector FMA latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latvecfma128wrapper));
  printf("Scalar FMA per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, scalarfmawrapper));
  printf("Scalar FMA latency: %.2f clocks\n", 1 / measureFunction(latencyIterations, clkSpeed, latscalarfmawrapper));
  printf("1:1 mixed 128-bit vector FMA/FADD per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixvecfaddfma128wrapper));
  printf("1:1 mixed 128-bit vector FMA/FMUL per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixvecfmulfma128wrapper));
  
  return 0;
}

/*Measures the execution time of the test specified, assuming a fixed clock speed.
*Then calculates the number of operations executed per clk as a measure of throughput.
*Returns the clk speed if the test was clktest, otherwise returns the opsperclk
*Param uint64_t iterations: the number of iterations the test should run through
*Param float clkspeed: the recorded clock frequency of the CPU for the test.
*Param uint64t (*testfunc) uint64_t: a pointer to the test function to be executed
*/
float measureFunction(uint64_t iterations, float clkSpeed, __attribute((sysv_abi)) uint64_t (*testfunc)(uint64_t))
{
  //Time structs for sys/time.h
  struct timeval startTv, endTv;
  struct timezone startTz, endTz;

  uint64_t time_diff_ms, retval;
  float latency, opsPerNs;

  gettimeofday(&startTv, &startTz);//Start timing
  retval = testfunc(iterations);//Assembly Test Execution
  gettimeofday(&endTv, &endTz);//Stop timing

  //Calculate the ops per iteration, or if clktest, the clock speed
  time_diff_ms = 1000 * (endTv.tv_sec - startTv.tv_sec) + ((endTv.tv_usec - startTv.tv_usec) / 1000);
  latency = 1e6 * (float)time_diff_ms / (float)iterations;
  opsPerNs = 1 / latency;
  
  //Determine if outputting the clock speed or the op rate by checking whether clktest was run
  if(testfunc == clktest)
  {
    clkSpeed = opsPerNs;
    return clkSpeed;
  }
  else
    return opsPerNs / clkSpeed;
}

//A series of currently necessary wrapper functions to avoid array loading issues in the load/store tests
uint64_t vecadd128wrapper(uint64_t iterations) {
  return vecadd128test(iterations, intTestArr);
}

uint64_t vecmul128wrapper(uint64_t iterations) {
  return vecmul128test(iterations, intTestArr);
}

uint64_t latvecadd128wrapper(uint64_t iterations) {
  return latvecadd128test(iterations, intTestArr);
}

uint64_t latvecmul128wrapper(uint64_t iterations) {
  return latvecmul128test(iterations, intTestArr);
}

uint64_t mixvecaddmul128wrapper(uint64_t iterations) {
  return mixvecaddmul128test(iterations, intTestArr);
}

uint64_t faddwrapper(uint64_t iterations) {
  return faddtest(iterations, fpTestArr);
}

uint64_t latfaddwrapper(uint64_t iterations) {
  return latfaddtest(iterations, fpTestArr);
}

uint64_t latvecfadd128wrapper(uint64_t iterations) {
  return latvecfadd128test(iterations, fpTestArr);
}

uint64_t latvecfmul128wrapper(uint64_t iterations) {
  return latvecfmul128test(iterations, fpTestArr);
}

uint64_t vecfadd128wrapper(uint64_t iterations) {
  return vecfadd128test(iterations, fpTestArr);
}

uint64_t vecfmul128wrapper(uint64_t iterations) {
  return vecfmul128test(iterations, fpTestArr);
}
uint64_t mixvecfaddfmul128wrapper(uint64_t iterations) {
  return mixvecfaddfmul128test(iterations, fpTestArr);
}

uint64_t mixaddvecadd128wrapper(uint64_t iterations) {
  return mixaddvecadd128test(iterations, intTestArr);
}

uint64_t mix3to1addvecadd128wrapper(uint64_t iterations) {
  return mix3to1addvecadd128test(iterations, intTestArr);
}

uint64_t mix1to1addvecadd128wrapper(uint64_t iterations) {
  return mix1to1addvecadd128test(iterations, intTestArr);
}

uint64_t mixmulvecmulwrapper(uint64_t iterations) {
  return mixmulvecmultest(iterations, intTestArr);
}

uint64_t mixvecmulfmulwrapper(uint64_t iterations) {
  return mixvecmulfmultest(iterations, fpTestArr, intTestArr);
}

uint64_t mixvecaddfaddwrapper(uint64_t iterations) {
  return mixvecaddfaddtest(iterations, fpTestArr, intTestArr);
}

uint64_t mixjmpvecaddwrapper(uint64_t iterations) {
  return mixjmpvecaddtest(iterations, intTestArr);
}

uint64_t mixjmpvecmulwrapper(uint64_t iterations) {
  return mixjmpvecmultest(iterations, intTestArr);
}

uint64_t vecloadwrapper(uint64_t iterations) {
  return vecloadtest(iterations, intTestArr);
}

uint64_t vecstorewrapper(uint64_t iterations) {
  return vecstoretest(iterations, intTestArr, sinkArr);
}

uint64_t loadwrapper(uint64_t iterations) {
  if (((uint64_t)intTestArr & 63) != 0) {
    printf("Warning - load may not be 64B aligned\n");
  }

  return loadtest(iterations, intTestArr);
}

uint64_t mixloadstorewrapper(uint64_t iterations) {
  return mixloadstoretest(iterations, intTestArr, sinkArr);
}

uint64_t mix21loadstorewrapper(uint64_t iterations) {
  return mix21loadstoretest(iterations, intTestArr, sinkArr);
}

uint64_t vecfma128wrapper(uint64_t iterations) {
  return vecfma128test(iterations, fpTestArr);
}

uint64_t scalarfmawrapper(uint64_t iterations) {
  return scalarfmatest(iterations, fpTestArr);
}

uint64_t latscalarfmawrapper(uint64_t iterations) {
  return latscalarfmatest(iterations, fpTestArr);
}

uint64_t latvecfma128wrapper(uint64_t iterations) {
  return latvecfma128test(iterations, fpTestArr);
}

uint64_t mixvecfmulfma128wrapper(uint64_t iterations) {
  return mixvecfmulfma128test(iterations, fpTestArr);
}

uint64_t mixvecfaddfma128wrapper(uint64_t iterations) {
  return mixvecfaddfma128test(iterations, fpTestArr);
}
