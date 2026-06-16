// this is a partial C file that's appended into generated code

// Run a test, return the result in time (ns or cycles) per branch
// historyLen: length of random array that the test loops through
// branchCountIdx: index into array of branch counts, max determined by generated header/asm
// random: if 1, randomize test array contents. If 0, fill with zeros
float runBranchHistTest(uint32_t historyLen, uint32_t branchCountIdx, int random) {
    uint32_t branchCount = branchCounts[branchCountIdx];
    uint64_t iterations = 33554432 / branchCount;
    uint64_t(*branchtestFunc)(uint64_t, uint32_t*, uint32_t*) __attribute((sysv_abi)) = branchtestFuncArr[branchCountIdx];

    uint32_t* testArrToArr = (uint32_t*)malloc(sizeof(uint32_t) * branchCount * historyLen);
    uint32_t* testArrToArrEnd = testArrToArr + branchCount * historyLen;

    for (int testArrIdx = 0; testArrIdx < branchCount * historyLen; testArrIdx += branchCount) {
        for (uint32_t i = 0; i < branchCount; i++) {
            testArrToArr[testArrIdx + i] = random ? rand() % 2 : 0;
        }
    }

    // warm up
    branchtestFunc(iterations, testArrToArr, testArrToArrEnd);
    GET_TIME_START();
    branchtestFunc(iterations, testArrToArr, testArrToArrEnd);
    GET_TIME_END();
    CALC_TIME_DIFF();
    float latency = (double)TIME_DIFF_VALUE / (double)(iterations);;

    // give result in latency per branch
    latency = latency / branchCount;

    free(testArrToArr);
    return latency;
}
