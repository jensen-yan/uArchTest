# Project Name: uArchTest

## Overview

uArchTest is a comprehensive collection of subprojects aimed at measuring and analyzing various performance metrics across different architectures, specifically x86 and ARM. This suite is designed for developers and researchers interested in performance optimization, benchmarking, and microarchitectural analysis. The suite includes six distinct subprojects, each focusing on a specific aspect of performance evaluation.

## Prerequisites

Before building and running the uArchTest suite, ensure that you have the following prerequisites installed on your system:

- **x86-64 Toolchain**: A toolchain for compiling and building applications for x86-64 architecture (e.g., GCC or Clang).
- **AArch64 Toolchain**: A toolchain for compiling and building applications for AArch64 architecture (e.g., GCC or Clang).
- **.NET SDK**: The .NET SDK is required for any .NET-related components or projects within the suite.
- **Make**: The `make` utility is needed to build the projects using the provided Makefile.

## Subprojects

### 1. AsmGen
AsmGen is a tool for generating assembly code and compiling it for x86 and ARM architectures. It facilitates testing and benchmarking of microarchitectural features through predefined tests, making it an essential resource for performance analysis at the assembly level.

### 2. CoherencyLatency
The CoherencyLatency project measures and analyzes the latency of thread synchronization mechanisms in multi-threaded environments. It focuses on PThreads, providing a cross-platform application that can be compiled for both x86 and ARM architectures.

### 3. CoreClockChecker
CoreClockChecker is designed to measure clock frequency on x86 and ARM architectures. It consists of two main components: `CoreClockChecker` and `BoostClockChecker`, allowing developers to evaluate clock performance across different hardware platforms.

### 4. InstructionRate
The InstructionRate project measures and analyzes the instruction execution rate on x86 and ARM architectures. It provides insights into the performance characteristics of various instruction sets, helping developers optimize their code for better efficiency.

### 5. Memory Bandwidth Benchmark
This project tests memory bandwidth using C and assembly code. It includes a version for Linux that utilizes POSIX threads for multithreading, providing a robust framework for evaluating memory performance.

### 6. Memory Latency Benchmark
The Memory Latency Benchmark measures memory latency across x86 and ARM architectures. It provides critical insights into the time required for memory operations, which is essential for understanding the performance characteristics of memory-intensive applications.

## Build Instructions

To build the entire suite, you can use the provided Makefile. The Makefile is designed to handle the compilation of all subprojects efficiently. Follow the steps below to build the projects:

1. **Clone the repository:**
   ```bash
   git clone https://git.woa.com/leytonzhang/uArchTest.git
   cd uArchTest
   ```

2. **Build the projects:**
   ```bash
   make
   ```

3. **Clean the build:**
   If you need to clean the build artifacts, you can run:
   ```bash
   make clean
   ```

4. **Build specific subprojects:**
   If you want to build a specific subproject, you can navigate to its directory and run:
   ```bash
   make
   ```

## Usage

Each subproject comes with its own set of instructions for usage. Please refer to the individual subproject directories for detailed documentation on how to run and utilize the tools effectively.

## Contributing

Contributions to uArchTest are welcome! If you have suggestions, improvements, or bug fixes, please feel free to submit a pull request or open an issue.

## License

This project is licensed under the Apache-2.0 License. See the LICENSE file for more details.

## Acknowledgments

We would like to thank all contributors and the open-source community for their support and collaboration in making this project possible.

## Original Author and Repository

This project was originally developed by [ChipsandCheese]. You can find the original repository at: [https://github.com/ChipsandCheese/Microbenchmarks].