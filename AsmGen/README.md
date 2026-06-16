# AsmGen

AsmGen is a subproject designed to generate assembly code and compile it for
different architectures. x86 has the broadest coverage; ARM and RISC-V have
partial coverage, with the basic integer, memory-queue, branch, and throughput
tests being the most useful starting points. The primary goal of AsmGen is to
facilitate the testing and benchmarking of various microarchitectural features
through a series of predefined tests.

## Project Overview

AsmGen automates the process of generating C and assembly files, compiling them for different architectures, and running a suite of tests that evaluate various aspects of CPU performance. By leveraging the capabilities of the .NET runtime and the GCC compiler, AsmGen provides a flexible and efficient way to explore microarchitectural behaviors.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Installing .NET](#installing-net)
- [Build Instructions](#build-instructions)
- [Usage](#usage)
- [Cleaning Up](#cleaning-up)
- [Conclusion](#conclusion)

## Prerequisites

Before you begin, ensure you have the following installed on your system：

- **GCC**: The GNU Compiler Collection.

- **Cross Compile**:
  - For x86-64: `gcc-x86-64-linux-gnu`
  - For ARM (AArch64): `gcc-aarch64-linux-gnu`
  - For RISC-V: `riscv64-unknown-linux-gnu-gcc`

### Ubuntu (APT)

```bash
sudo apt update
sudo apt install -y gcc-x86-64-linux-gnu gcc-aarch64-linux-gnu
```

### CentOS/RHEL (YUM)

```bash
sudo yum install -y gcc-x86-64-linux-gnu gcc-aarch64-linux-gnu
```

## Installing .NET

To install the .NET SDK, follow the instructions for your Linux distribution:

### Ubuntu (APT)

1. Open a terminal and run the following commands to install the .NET SDK:
   ```bash
   sudo apt update
   sudo apt install -y dotnet-sdk-8.0
   ```
   *(Replace `8.0` with the desired version if necessary.)*

2. After installation, verify the installation by running:
   ```bash
   dotnet --version
   ```

### CentOS/RHEL (YUM)

1. Open a terminal and run the following commands to install the .NET SDK:
   ```bash
   sudo yum install -y dotnet-sdk-8.0
   ```
   *(Replace `8.0` with the desired version if necessary.)*

2. After installation, verify the installation by running:
   ```bash
   dotnet --version
   ```

For more detailed installation instructions, you can refer to the official Microsoft documentation: [Install .NET on Linux](https://docs.microsoft.com/dotnet/core/install/linux).

## Build Instructions

To build the project, you can use the provided Makefile. Follow these steps:

1. Navigate to the project directory.

2. Run one of the following commands:
   ```bash
   make ARCH=x86
   make ARCH=riscv TIMING=cycle ONLY=nopbw
   ```

This will execute the following steps:
- Check if the dotnet CLI is installed.
- Generate the necessary files using `dotnet run`.
- Compile the generated files for the selected architecture.

`ONLY=<prefix>` is useful for simulator experiments because the full combined
benchmark generates very large assembly files.

### Clean Up

To remove all generated files and binaries, run:
```bash
make clean
```

## Usage

After building the project, you can run the generated binaries with the following command:

```bash
./clammicrobench_x86 [test name] [latency list size] [struct iterations]
```

or

```bash
./clammicrobench_arm [test name] [latency list size] [struct iterations]
```

or

```bash
./clammicrobench_riscv [test name] [struct iterations]
```

### Parameters
- **test name**: The name of the test you want to run.
- **latency list size**: The size of the latency list (default is 33554432).
- **struct iterations**: The number of iterations for the structure (default is 5000000).

## Conclusion

AsmGen provides a streamlined way to generate and compile assembly code for different architectures while facilitating performance testing and analysis. Follow the instructions above to build and run the tests as needed. For any issues or contributions, feel free to open an issue or pull request in the repository.
