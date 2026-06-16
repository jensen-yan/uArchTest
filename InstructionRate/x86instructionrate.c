/* This is a series of x86-64 ISA micro-tests designed to identify and report the execution rate of various instructions on x86-64 processor.
 * The tests are principally in assembly, that is wrapped in C code to handle timing and reporting results
 * Clam did all the hard work, Nintonito just cleaned it up a bit.  Give him the real credit
 */
#include "x86instructionrate.h"

int main(int argc, char *argv[])
{
  //Base value for iterations.  Latency tests require a separate, higher value
  uint64_t iterations = 1500000000, latencyIterations = iterations * 5;
  
  float clkSpeed;//CPU Frequency

  //ISA Feature Flags
  int avxSupported, avx2Supported, bmi2Supported, avx512Supported, fmaSupported, fma4Supported;
  
  //Populate the int test array with numbers (avoid setting elements to 0)
  for (uint64_t i = 0; i < intTestArrLength; i++)
  {
    intTestArr[i] = i;
  }

  //Applies cmdline arg -iterations to the iteration variables as a scale factor
  if(argc > 2)
  {
    uint64_t scaleFactor = (uint64_t)atol(argv[2]);
    latencyIterations *= scaleFactor;
    iterations *= scaleFactor;
    printf("setting %lu iterations\n", latencyIterations);
  }

  //Block below checks for ISA Extension Availability
  avxSupported = __builtin_cpu_supports("avx");
  if (avxSupported > 0)
    fprintf(stderr, "avx supported\n");
  avx2Supported = __builtin_cpu_supports("avx2");
  if (avx2Supported > 0)
    fprintf(stderr, "avx2 supported\n");
  bmi2Supported = __builtin_cpu_supports("bmi2");
  if (bmi2Supported > 0)
    fprintf(stderr, "bmi2 supported\n");
  fmaSupported = __builtin_cpu_supports("fma");
  if (fmaSupported > 0)
    fprintf(stderr, "fma3 supported\n");
  fma4Supported = __builtin_cpu_supports("fma4");
  if (fma4Supported > 0)
    fprintf(stderr, "fma4 supported\n");
  avx512Supported = __builtin_cpu_supports("avx512f") && __builtin_cpu_supports("avx512dq");
  if (avx512Supported > 0)
    fprintf(stderr, "AVX512 supported\n");
  //End of ISA Extension Checks
  
  //Establish baseline clock speed for CPU, for all further calculations
  clkSpeed = measureFunction(latencyIterations, clkSpeed, clktest);
  printf("Estimated clock speed: %.2f GHz\n", clkSpeed);
  
  //Grouping Together all AVX-512 operations due to the sheer number
  if (avx512Supported)
  {
    if (argumentCheck(argc, argv[1], "fma512", 6) == 0)
      printf("512-bit FMA per clk: %.2f\n", measureFunction(iterations, clkSpeed, fma512));
    if (argumentCheck(argc, argv[1], "latfma512", 9) == 0)
      printf("512-bit FMA latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latfma512));
    if (argumentCheck(argc, argv[1], "mixfma256fma512", 15) == 0)
      printf("1:1 256-bit/512-bit FMA per clk: %.2f\n", measureFunction(iterations, clkSpeed, mixfma256fma512));
    if (argumentCheck(argc, argv[1], "mix21fma256fma512", 17) == 0)
      printf("2:1 256-bit/512-bit FMA per clk: %.2f\n", measureFunction(iterations, clkSpeed, mix21fma256fma512));
    if (argumentCheck(argc, argv[1], "nemesfpumix21", 13) == 0)
      printf("1:2 512b FMA:FADD per clk (nemes): %.2f\n", measureFunction(iterations * 22, clkSpeed, nemesfpu512mix21));
    if (argumentCheck(argc, argv[1], "add512int", 9) == 0)
      printf("512-bit int add per clk: %.2f\n", measureFunction(iterations, clkSpeed, add512int));
    if (argumentCheck(argc, argv[1], "latadd512int", 12) == 0)
      printf("512-bit int add latency: %.2f clocks\n", 1 / measureFunction(latencyIterations, clkSpeed, latadd256int));
    if (argumentCheck(argc, argv[1], "mul512int", 9) == 0)
      printf("512-bit 32-bit int mul per clk: %.2f\n", measureFunction(iterations, clkSpeed, mul512int));
    if (argumentCheck(argc, argv[1], "muldq512int", 9) == 0)
      printf("512-bit 32->64-bit int mul per clk: %.2f\n", measureFunction(iterations, clkSpeed, muldq512int));
    if (argumentCheck(argc, argv[1], "latmulq512int", 13) == 0)
      printf("512-bit 64-bit int mul latency: %.2f clocks\n", 1 / measureFunction(latencyIterations, clkSpeed, latmulq512int));
    if (argumentCheck(argc, argv[1], "latmul512int", 12) == 0)
      printf("512-bit 32-bit int mul latency: %.2f clocks\n", 1 / measureFunction(latencyIterations, clkSpeed, latmul512int));
    if (argumentCheck(argc, argv[1], "latmuldq512int", 13) == 0)
      printf("512-bit 32->64-bit int mul latency: %.2f clocks\n", 1 / measureFunction(latencyIterations, clkSpeed, latmuldq512int));
    if (argumentCheck(argc, argv[1], "mixfmaadd512", 11) == 0)
      printf("1:2 512b PADDQ:FMA per clk: %.2f\n", measureFunction(iterations * 22, clkSpeed, mixfmaadd512));
    if (argumentCheck(argc, argv[1], "mixfma512add256", 11) == 0)
      printf("1:2 256b PADDQ : 512b FMA per clk: %.2f\n", measureFunction(iterations * 22, clkSpeed, mixfma512add256));

    if (argumentCheck(argc, argv[1], "load512", 7) == 0)
      printf("512-bit loads per clk: %.2f\n", measureFunction(iterations, clkSpeed, load512wrapper));
    if (argumentCheck(argc, argv[1], "store512", 7) == 0)
      printf("512-bit stores per clk: %.2f\n", measureFunction(iterations, clkSpeed, store512wrapper));

    if (argumentCheck(argc, argv[1], "aesenc128", 9) == 0)
      printf("aesenc per clk: %.2f\n", measureFunction(iterations, clkSpeed, aesenc128));
    if (argumentCheck(argc, argv[1], "aesdec128", 9) == 0)
      printf("aesdec per clk: %.2f\n", measureFunction(iterations, clkSpeed, aesdec128));
    if (argumentCheck(argc, argv[1], "aesencadd128", 12) == 0)
      printf("1:3 aesenc+paddd per clk: %.2f\n", measureFunction(iterations, clkSpeed, aesencadd128));
    if (argumentCheck(argc, argv[1], "aesencfma128", 12) == 0)
      printf("1:2 aesenc+fma per clk: %.2f\n", measureFunction(iterations, clkSpeed, aesencfma128));
    if (argumentCheck(argc, argv[1], "aesencmul128", 12) == 0)
      printf("1:2 aesenc+pmullw per clk: %.2f\n", measureFunction(iterations, clkSpeed, aesencmul128));
    if (argumentCheck(argc, argv[1], "aesencmul128", 12) == 0)
      printf("1:2 aesenc+addps per clk: %.2f\n", measureFunction(iterations, clkSpeed, aesencfadd128));
  }

  if (argumentCheck(argc, argv[1], "1bnop", 5) == 0)
    printf("1-byte nops per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, noptest1b));
  if (argumentCheck(argc, argv[1], "2bnop", 5) == 0)
    printf("2-byte nops per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, noptest));
  if (argumentCheck(argc, argv[1], "add", 3) == 0)
    printf("Adds per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, addtest));
  if (argumentCheck(argc, argv[1], "addnop", 7) == 0)
    printf("1:4 nops/adds per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, addnoptest));
  if (argumentCheck(argc, argv[1], "addmov", 7) == 0)
    printf("1:4 movs/adds per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, addnoptest));

  if (argumentCheck(argc, argv[1], "depmov", 6) == 0)
    printf("Dependent movs per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, depmovtest));
  if (argumentCheck(argc, argv[1], "indepmov", 8) == 0)
    printf("Independent movs per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, indepmovtest));
  if (argumentCheck(argc, argv[1], "xorzero", 7) == 0)
    printf("xor -> 0 per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, xorzerotest));
  if (argumentCheck(argc, argv[1], "movzero", 7) == 0)
    printf("mov -> 0 per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, movzerotest));
  if (argumentCheck(argc, argv[1], "subzero", 7) == 0)
    printf("sub -> 0 per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, subzerotest));
  if (argumentCheck(argc, argv[1], "depinc", 6) == 0)
    printf("dep inc per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, depinctest));
  if (argumentCheck(argc, argv[1], "depdec", 6) == 0)
    printf("dep dec per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, depdectest));
  if (argumentCheck(argc, argv[1], "depdec", 6) == 0)
    printf("dep add immediate per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, depaddimmtest));
  if (argumentCheck(argc, argv[1], "clkmov", 6) == 0)
    printf("dep add + mov pair per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, clkmovtest));
  if (argumentCheck(argc, argv[1], "vecdepmov", 9) == 0)
    printf("Dependent vec movs per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecdepmovtest));
  if (argumentCheck(argc, argv[1], "vecindepmov", 12) == 0)
    printf("Independent vec movs per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecindepmovtest));
  if (argumentCheck(argc, argv[1], "vecxorzero", 10) == 0)
    printf("xor xmm -> 0 per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecxorzerotest));
  if (argumentCheck(argc, argv[1], "vecsubzero", 10) == 0)
    printf("sub xmm -> 0 per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, vecsubzerotest));

  if (argumentCheck(argc, argv[1], "miximuladd", 10) == 0)
    printf("4:1 adds/imul per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, addmultest));
  if (argumentCheck(argc, argv[1], "jmpmul", 6) == 0)
    printf("1:1 mul/jmp per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, jmpmultest));
  if (argumentCheck(argc, argv[1], "jmp", 3) == 0)
    printf("taken jmp per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, jmptest));
  if (argumentCheck(argc, argv[1], "ntjmp", 5) == 0)
    printf("nt jmp per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, ntjmptest));
  
  //Testing Bit Manipulation Instructions
  if (bmi2Supported > 0)
  {
    if (argumentCheck(argc, argv[1], "pdep", 4) == 0)
      printf("pdep per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, pdeptest));
    if (argumentCheck(argc, argv[1], "pext", 4) == 0)
      printf("pext per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, pexttest));
    if (argumentCheck(argc, argv[1], "pdepmul", 7) == 0)
      printf("1:1 pdep/mul per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, pdepmultest));
  }

  if (argumentCheck(argc, argv[1], "shl", 3) == 0)
    printf("shl r,1 per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, shltest));
  if (argumentCheck(argc, argv[1], "ror", 3) == 0)
    printf("ror r,1 per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, rortest));
  if (argumentCheck(argc, argv[1], "mixrorshl", 9) == 0)
    printf("1:1 shl/ror r,1 per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, mixrorshltest));
  if (argumentCheck(argc, argv[1], "mixrormul", 3) == 0)
    printf("1:1 ror/mul per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, mixrormultest));
  if (argumentCheck(argc, argv[1], "bts", 3) == 0)
    printf("bts per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, btstest));
  if (argumentCheck(argc, argv[1], "mixmulbts", 9) == 0)
    printf("1:1 bts/mul per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, btsmultest));
  if (argumentCheck(argc, argv[1], "mixrorbts", 9) == 0)
    printf("1:1 bts/ror per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, rorbtstest));
  if (argumentCheck(argc, argv[1], "lea", 3) == 0)
    printf("lea r+r*8 per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, leatest));
  if (argumentCheck(argc, argv[1], "mixmullea", 9) == 0)
    printf("1:1 lea r+r*8/mul per clk: %.4f\n", measureFunction(latencyIterations, clkSpeed, leamultest));
  
  //256b AVX ALU Rate Tests
  if(avx2Supported > 0)
  {
    if (argumentCheck(argc, argv[1], "avx256int", 9) == 0)
      printf("256-bit avx integer add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, add256int));
    if (argumentCheck(argc, argv[1], "mixavx256int", 12) == 0)
      printf("2:1 scalar add/256-bit avx integer add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixadd256int));
    if (argumentCheck(argc, argv[1], "mix11avx256int", 14) == 0)
      printf("1:1 scalar add/256-bit avx integer add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixadd256int11));
    if (argumentCheck(argc, argv[1], "mixavx256fpint", 14) == 0)
      printf("1:1 256-bit avx int add/avx fadd per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mixadd256fpint));
    if (argumentCheck(argc, argv[1], "fadd256", 6) == 0)
      printf("256-bit FADD per clk: %.2f\n", measureFunction(iterations, clkSpeed, add256fp));
    if (argumentCheck(argc, argv[1], "fmul256", 6) == 0)
      printf("256-bit FMUL per clk: %.2f\n", measureFunction(iterations, clkSpeed, mul256fp));
  }

  if (avxSupported > 0 && (argumentCheck(argc, argv[1], "mix256fp", 8) == 0))
    printf("1:1 256-bit avx fp mul/add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mix256fp));
  
  //256b AVX add/mul latency tests
  if(avx2Supported > 0)
  {
    if (argumentCheck(argc, argv[1], "latadd256int", 12) == 0)
      printf("256-bit avx2 integer add latency: %.2f clocks\n", 1 / measureFunction(latencyIterations, clkSpeed, latadd256int));
    if (argumentCheck(argc, argv[1], "latmul256int", 12) == 0)
      printf("256-bit avx2 integer multiply latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latmul256int));
  }
  
  //128b SSE add/mul latency tests
  if (argumentCheck(argc, argv[1], "latadd128int", 12) == 0)
    printf("128-bit sse integer add latency: %.2f clocks\n", 1 / measureFunction(latencyIterations, clkSpeed, latadd128int));
  if (argumentCheck(argc, argv[1], "latmul128int", 12) == 0)
    printf("128-bit sse integer multiply latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latmul128int));
  
  //256b AVX fadd/fmul latency tests
  if(avxSupported > 0)
  {
    if (avxSupported > 0 && (argumentCheck(argc, argv[1], "latadd256fp", 11) == 0))
      printf("256-bit avx fadd latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latadd256fp));
    if (avxSupported > 0 && (argumentCheck(argc, argv[1], "latmul256fp", 11) == 0))
      printf("256-bit avx fmul latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latmul256fp));
  }
  //SSE float and int add/mul tests
  if (argumentCheck(argc, argv[1], "latadd128fp", 11) == 0)
    printf("128-bit sse fadd latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latadd128fp));
  if (argumentCheck(argc, argv[1], "latmul128fp", 11) == 0)
    printf("128-bit sse fmul latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latmul128fp));
  if (argumentCheck(argc, argv[1], "add128fp", 8) == 0)
    printf("128-bit sse fadd per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, add128fp));
  if (argumentCheck(argc, argv[1], "mul128fp", 8) == 0)
    printf("128-bit sse fmul per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mul128fp));
  if (argumentCheck(argc, argv[1], "add128int", 9) == 0)
    printf("128-bit sse int add per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, add128int));
  if (argumentCheck(argc, argv[1], "mul128int", 9) == 0)
    printf("128-bit sse int mul per clk: %.2f\n", measureFunction(latencyIterations, clkSpeed, mul128int));

  //FMA3 Tests.  Note that AVX is also required despite fma being a separate flag
  if (fmaSupported > 0)
  {
    if (avxSupported > 0)
    {
      if (argumentCheck(argc, argv[1], "fma256", 6) == 0)
        printf("256-bit FMA per clk: %.2f\n", measureFunction(iterations, clkSpeed, fma256));
      if (argumentCheck(argc, argv[1], "fma128", 6) == 0)
        printf("128-bit FMA per clk: %.2f\n", measureFunction(iterations, clkSpeed, fma128));
      if (argumentCheck(argc, argv[1], "latfma256", 9) == 0)
        printf("256-bit FMA latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latfma256));
      if (argumentCheck(argc, argv[1], "latfma128", 9) == 0)
        printf("128-bit FMA latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latfma128));
      if (argumentCheck(argc, argv[1], "mixfmafadd256", 12) == 0)
        printf("1:2 256b FMA:FADD per clk: %.2f\n", measureFunction(iterations * 22, clkSpeed, mixfmafadd256));
      if (argumentCheck(argc, argv[1], "mixfmaadd256", 11) == 0)
        printf("2:1 256b FMA:PADDQ per clk: %.2f\n", measureFunction(iterations * 22, clkSpeed, mixfmaadd256));
      if (argumentCheck(argc, argv[1], "mixfmaandmem256", 14) == 0)
        printf("2:1 256b FMA:PADDQ load-op per clk: %.2f\n", measureFunction(iterations * 22, clkSpeed, mixfmaaddmem256wrapper));
      if (argumentCheck(argc, argv[1], "mixfmaand256", 11) == 0)
        printf("2:1 256b FMA:PAND per clk: %.2f\n", measureFunction(iterations * 22, clkSpeed, mixfmaand256));
      if (argumentCheck(argc, argv[1], "mixfmaandmem256", 14) == 0)
        printf("2:1 256b FMA:PAND load-op per clk: %.2f\n", measureFunction(iterations * 22, clkSpeed, mixfmaandmem256wrapper));
      if (argumentCheck(argc, argv[1], "nemesfpumix21", 13) == 0)
        printf("1:2 256b FMA:FADD per clk (nemes): %.2f\n", measureFunction(iterations * 22, clkSpeed, nemesfpumix21));
      if (argumentCheck(argc, argv[1], "mix256faddintadd", 15) == 0)
        printf("1:2 256b FMA:PADD per clk: %.2f\n", measureFunction(iterations, clkSpeed, mix256faddintadd));
    }
  }
  //FMA4 Tests for select AMD CPU's
  if (fma4Supported > 0)
  {
    if (argumentCheck(argc, argv[1], "fma4_256", 8) == 0)
      printf("256-bit FMA4 per clk: %.2f\n", measureFunction(iterations, clkSpeed, fma4_256));
    if (argumentCheck(argc, argv[1], "fma4_256", 8) == 0)
      printf("128-bit FMA4 per clk: %.2f\n", measureFunction(iterations, clkSpeed, fma4_128));
  }
  //imul tests
  if (argumentCheck(argc, argv[1], "latmul16", 8) == 0)
    printf("16-bit imul latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latmul16));
  if (argumentCheck(argc, argv[1], "latmul64", 8) == 0)
    printf("64-bit imul latency: %.2f clocks\n", 1 / measureFunction(iterations, clkSpeed, latmul64));
  if (argumentCheck(argc, argv[1], "mul16", 5) == 0)
    printf("16-bit imul per clk: %.2f\n", measureFunction(iterations, clkSpeed, mul16));
  if (argumentCheck(argc, argv[1], "mul64", 5) == 0)
    printf("64-bit imul per clk: %.2f\n", measureFunction(iterations, clkSpeed, mul64));
  if (argumentCheck(argc, argv[1], "mixmul16mul64", 5) == 0)
    printf("1:1 mixed 16-bit/64-bit imul per clk: %.2f\n", measureFunction(iterations, clkSpeed, mixmul16mul64));
  if (argumentCheck(argc, argv[1], "mix21mul16mul64", 5) == 0)
    printf("2:1 mixed 16-bit/64-bit imul per clk: %.2f\n", measureFunction(iterations, clkSpeed, mixmul16mul64_21));

  //Testing 128bit and 256bit loads/stores
  if (argumentCheck(argc, argv[1], "load128", 7) == 0)
    printf("128-bit loads per clk: %.2f\n", measureFunction(iterations, clkSpeed, load128wrapper));
  if (argumentCheck(argc, argv[1], "spacedload128", 13) == 0)
    printf("128-bit loads (spaced) per clk: %.2f\n", measureFunction(iterations, clkSpeed, spacedload128wrapper));
  if (avxSupported > 0 && (argumentCheck(argc, argv[1], "load256", 7) == 0))
    printf("256-bit loads per clk: %.2f\n", measureFunction(iterations, clkSpeed, load256wrapper));
  if (argumentCheck(argc, argv[1], "spacedstorescalar", 13) == 0)
    printf("scalar stores (spaced) per clk: %.2f\n", measureFunction(iterations, clkSpeed, spacedstorescalarwrapper));
  if (argumentCheck(argc, argv[1], "store128", 7) == 0)
    printf("128-bit stores per clk: %.2f\n", measureFunction(iterations, clkSpeed, store128wrapper));
  if (avxSupported > 0 && argumentCheck(argc, argv[1],  "store256", 7) == 0)
    printf("256-bit stores per clk: %.2f\n", measureFunction(iterations, clkSpeed, store256wrapper));
  if (argumentCheck(argc, argv[1], "mixaddmul128int", 15) == 0)
    printf("1:1 mixed 128-bit vec add/mul per clk: %.2f\n", measureFunction(iterations, clkSpeed, mixaddmul128int));
  
  //Testing for instruction fusion
  if (argumentCheck(argc, argv[1], "testfusion", 10) == 0)
    printf("test+jnz: %.2f\n", measureFunction(iterations, clkSpeed, testfusion));
  if (argumentCheck(argc, argv[1], "cmpfusion", 9) == 0)
    printf("cmp+jnz: %.2f\n", measureFunction(iterations, clkSpeed, cmpfusion));
  if (argumentCheck(argc, argv[1], "subfusion", 9) == 0)
    printf("sub+jnz: %.2f\n", measureFunction(iterations, clkSpeed, subfusion));
  if (argumentCheck(argc, argv[1], "nopfusion", 9) == 0)
    printf("nop+jnz: %.2f\n", measureFunction(iterations, clkSpeed, nopfusion));

  return 0;
}

/* A macro function for a common if test done before deciding to run a test.  
*Checks value of argc, and if greater then 1 it checks if argument and test are equal.
*Param int argc: Should be the argc provided to main()
*Param char* argument: Should be argv[n] provided to main()
*Param char* test: The string of the test being evaluated
*Param size_t argLength: The length of the "test" param string
*/
static inline int argumentCheck(int argc, char* argument, char* test, size_t argLength)
{
  if(argc > 1)
    return (strncmp(argument, test, argLength));
  else
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

//A series of currently necessary wrapper functions to avoid function parameter mismatching with the measure function
__attribute((sysv_abi)) uint64_t load128wrapper(uint64_t iterations)
{
  return load128(iterations, intTestArr);
}

__attribute((sysv_abi)) uint64_t spacedload128wrapper(uint64_t iterations)
{
  return spacedload128(iterations, intTestArr);
}

__attribute((sysv_abi)) uint64_t spacedstorescalarwrapper(uint64_t iterations)
{
  return spacedstorescalar(iterations, intTestArr);
}

__attribute((sysv_abi)) uint64_t load256wrapper(uint64_t iterations)
{
  return load256(iterations, fpTestArr);
}

__attribute((sysv_abi)) uint64_t load512wrapper(uint64_t iterations)
{
  return load512(iterations, fpTestArr);
}

__attribute((sysv_abi)) uint64_t spacedload256wrapper(uint64_t iterations)
{
  return spacedload128(iterations, intTestArr);
}

__attribute((sysv_abi)) uint64_t store128wrapper(uint64_t iterations)
{
  return store128(iterations, intTestArr, intSinkArr);
}

__attribute((sysv_abi)) uint64_t store256wrapper(uint64_t iterations)
{
  return store256(iterations, fpTestArr, fpSinkArr);
}

__attribute((sysv_abi)) uint64_t store512wrapper(uint64_t iterations)
{
  return store512(iterations, fpTestArr, fpSinkArr);
}

__attribute((sysv_abi)) uint64_t mixfmaandmem256wrapper(uint64_t iterations)
{
  return mixfmaandmem256(iterations, fpTestArr);
}

__attribute((sysv_abi)) uint64_t mixfmaaddmem256wrapper(uint64_t iterations)
{
  return mixfmaaddmem256(iterations, fpTestArr);
}
