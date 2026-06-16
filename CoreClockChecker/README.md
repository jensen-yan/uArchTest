# CoreClockChecker

## Project Overview

The CoreClockChecker project is a C-based application designed to measure clock frequency in both x86 and ARM architectures. The project includes two main components: `CoreClockChecker` and `BoostClockChecker`, which are compiled separately for each architecture. This allows developers to evaluate clock performance across different hardware platforms, making it a valuable tool for performance analysis and optimization.

## Prerequisites

Before you begin, ensure you have the following installed on your system:

- **GNU Compiler Collection (GCC)**: The project uses `gcc` for compilation.
- **Cross-compilers**: You will need the appropriate cross-compilers for x86 and ARM architectures:
  - `x86_64-linux-gnu-gcc` for x86 architecture
  - `aarch64-linux-gnu-gcc` for ARM architecture
- **Make**: The `make` utility is required to run the Makefile.

## Building the Project

To build the project, follow these steps:

1. **Navigate to the Project Directory**:
   Open a terminal and change to the directory containing the Makefile.

2. **Run the Makefile**:
   To compile the project for both architectures, simply run:
   ```bash
   make
   ```

   This will execute the `all` target, which compiles the `CoreClockChecker` and `BoostClockChecker` for both x86 and ARM architectures.

3. **Output**:
   After a successful build, you will find the following binaries in the project directory:
   - `CoreClockChecker_x86`: The compiled binary for the CoreClockChecker on x86 architecture.
   - `BoostClockChecker_x86`: The compiled binary for the BoostClockChecker on x86 architecture.
   - `BoostClockChecker_arm`: The compiled binary for the BoostClockChecker on ARM architecture.

## Cleaning Up

To remove the generated binaries and clean up the project directory, run:
```bash
make clean
```
This will delete the compiled binaries and any other generated files.

## Customizing Compiler Flags

If you need to customize the compiler flags, you can modify the `CFLAGS` variable in the Makefile. The default optimization level is set to `-O2`. You can add additional flags as needed.

## Troubleshooting

- If you encounter issues with missing cross-compilers, ensure that they are installed and available in your system's PATH.
- If you receive errors during compilation, check the source code for any syntax errors or missing dependencies.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.

## Contact

For any questions or issues, please contact the project maintainer at [your-email@example.com].