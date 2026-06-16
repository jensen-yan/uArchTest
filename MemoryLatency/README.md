# Memory Latency Benchmark

## Overview

The Memory Latency Benchmark is a project designed to measure the memory latency of systems across different architectures, specifically x86 and ARM. This benchmark provides insights into the time it takes for memory operations to complete, which is crucial for understanding the performance characteristics of applications that rely heavily on memory access.

## Project Structure

The project consists of the following key components:

- **Source Files**: 
  - `MemoryLatency.c`: The main source file containing the benchmark logic.
  - `MemoryLatency_x86.s`: Assembly code specific to the x86 architecture.
  - `MemoryLatency_arm.s`: Assembly code specific to the ARM architecture.

- **Makefile**: The Makefile automates the build process for both architectures, handling compilation and linking.

## Makefile Overview

The provided Makefile includes rules for compiling the benchmark for both x86 and ARM architectures. Below are the key sections of the Makefile:

### Variables

- **CC**: The C compiler to use (set to `gcc`).
- **CROSS_COMPILE_AARCH64**: Prefix for the ARM cross-compiler.
- **CROSS_COMPILE_X86_64**: Prefix for the x86 cross-compiler.

### Compiler and Linker Flags

- **CFLAGS**: Compiler flags, currently set to optimize the code with `-O2`.
- **LDFLAGS**: Linker flags for static linking, threading, and math library support.

### Compilation Rules

- **compile_x86**: Compiles the benchmark for the x86 architecture.
- **compile_arm**: Compiles the benchmark for the ARM architecture.

### Default Target

- **all**: The default target that compiles both x86 and ARM binaries.

### Clean Rule

- **clean**: Removes generated binaries and cleans up the project directory.

## Building the Project

To compile the Memory Latency Benchmark, follow these steps:

1. Open a terminal and navigate to the directory containing the Makefile.
2. Run the following command to build the project for both architectures:

   ```bash
   make
   ```

This will generate the following binaries:

- `MemoryLatency_x86`: The compiled binary for x86 architecture.
- `MemoryLatency_arm`: The compiled binary for ARM architecture.

## Usage

After compiling the project, you can run the benchmark using the generated binaries. The benchmark accepts the following command-line options:

```
Usage: [-test <c/asm/tlb/mlp>] [-maxsizemb <max test size in MB>] [-iter <base iterations, default 100000000>]
```

### Command-Line Options

- **-test <type>**: Specify the type of test to run. Valid test types include:
  - `c`: C-based memory latency test.
  - `asm`: Assembly-based memory latency test.
  - `tlb`: Translation Lookaside Buffer test.
  - `mlp`: Memory Level Parallelism test.
  - `stlf`: Store to Load Forwarding test.
  - `matched_stlf`: Matched Store to Load Forwarding  test.
  - `dword_stlf`: Double Word Store to Load Forwarding  test.

- **-maxsizemb <size>**: Set the maximum test size in megabytes (MB).

- **-iter <iterations>**: Set the number of base iterations for the test. The default value is `100000000`.

### Example Command

To run a C-based memory latency test with a maximum size of 512 MB and 200 million iterations, you would use:

```bash
./MemoryLatency_x86 -test c -maxsizemb 512 -iter 200000000
```

## Cleaning Up

To remove the compiled binaries and clean the project directory, run:

```bash
make clean
```

This will delete the `MemoryLatency_x86` and `MemoryLatency_arm` files.
