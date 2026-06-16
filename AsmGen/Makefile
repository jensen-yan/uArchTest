# AsmGen/Makefile - Simplified version
.PHONY: default all separate clean help

# Default target
default: all

# Variables
CC := gcc
TIMING ?= ns
ARCH ?= all
USE_KLIB ?= no
CFLAGS := -O2

# Cross-compilers
CROSS_COMPILE_AARCH64 := aarch64-linux-gnu-
CROSS_COMPILE_X86_64 := x86_64-linux-gnu-
CROSS_COMPILE_RISCV64 := riscv64-unknown-linux-gnu-

# Linker flags based on timing mode
ifeq ($(TIMING),cycle)
    LDFLAGS := -static -lm
else
    LDFLAGS := -static -lm -lrt
endif

# Check if dotnet is installed
check-dotnet:
	@if ! command -v dotnet > /dev/null; then \
		echo "Error: dotnet CLI is not installed"; \
		exit 1; \
	fi

# Generate and compile combined tests
all: check-dotnet
	@echo "Generating and compiling combined tests (arch: $(ARCH), timing: $(TIMING), klib: $(USE_KLIB))..."
	@$(if $(filter yes,$(USE_KLIB)), \
		dotnet run --target-arch $(ARCH) --timing $(TIMING) --use-klib, \
		dotnet run --target-arch $(ARCH) --timing $(TIMING))
	@$(MAKE) compile

# Generate and compile separate tests  
separate: check-dotnet
	@echo "Generating and compiling separate tests (arch: $(ARCH), timing: $(TIMING), klib: $(USE_KLIB))..."
	@$(if $(filter yes,$(USE_KLIB)), \
		dotnet run --separate --target-arch $(ARCH) --timing $(TIMING) --use-klib, \
		dotnet run --separate --target-arch $(ARCH) --timing $(TIMING))
	@$(MAKE) compile_separate

# Compile generated combined tests
compile:
	@echo "Compiling combined tests..."
	@cd generate && \
	if [ -f clammicrobench_x86.S ]; then \
		$(CROSS_COMPILE_X86_64)$(CC) $(CFLAGS) clammicrobench.c clammicrobench_x86.S latency_test_data.S -o clammicrobench_x86 $(LDFLAGS); \
		echo "Built clammicrobench_x86"; \
	fi && \
	if [ -f clammicrobench_arm.S ]; then \
		$(CROSS_COMPILE_AARCH64)$(CC) $(CFLAGS) clammicrobench.c clammicrobench_arm.S latency_test_data.S -o clammicrobench_arm $(LDFLAGS); \
		echo "Built clammicrobench_arm"; \
	fi && \
	if [ -f clammicrobench_riscv.S ]; then \
		$(CROSS_COMPILE_RISCV64)$(CC) $(CFLAGS) clammicrobench.c clammicrobench_riscv.S latency_test_data.S -o clammicrobench_riscv $(LDFLAGS); \
		echo "Built clammicrobench_riscv"; \
	fi

# Compile generated separate tests (parallel)
compile_separate:
	@echo "Compiling separate tests in parallel..."
	@find generate -maxdepth 1 -type d -not -path generate | xargs -I {} -P $(shell nproc) sh -c ' \
		if [ -d "{}" ]; then \
			test_name=$$(basename "{}"); \
			cd "{}" && \
			if [ -f "$${test_name}_test_x86.S" ]; then \
				$(CROSS_COMPILE_X86_64)$(CC) $(CFLAGS) "$${test_name}_test.c" "$${test_name}_test_x86.S" latency_test_data.S -o "$${test_name}_test_x86" $(LDFLAGS) && \
				echo "Built $${test_name}_test_x86"; \
			fi && \
			if [ -f "$${test_name}_test_arm.S" ]; then \
				$(CROSS_COMPILE_AARCH64)$(CC) $(CFLAGS) "$${test_name}_test.c" "$${test_name}_test_arm.S" latency_test_data.S -o "$${test_name}_test_arm" $(LDFLAGS) && \
				echo "Built $${test_name}_test_arm"; \
			fi && \
			if [ -f "$${test_name}_test_riscv.S" ]; then \
				$(CROSS_COMPILE_RISCV64)$(CC) $(CFLAGS) "$${test_name}_test.c" "$${test_name}_test_riscv.S" latency_test_data.S -o "$${test_name}_test_riscv" $(LDFLAGS) && \
				echo "Built $${test_name}_test_riscv"; \
			fi \
		fi'

# Clean generated files
clean:
	@echo "Cleaning up..."
	@rm -rf generate/ bin/ obj/

# Help
help:
	@echo "Usage: make [target] [ARCH=arch] [TIMING=timing] [USE_KLIB=yes/no]"
	@echo ""
	@echo "Targets:"
	@echo "  all      - Generate and compile combined tests (default)"
	@echo "  separate - Generate and compile separate tests"  
	@echo "  clean    - Clean all generated files"
	@echo "  help     - Show this help"
	@echo ""
	@echo "Variables:"
	@echo "  ARCH     - Target architecture: all, x86, arm, riscv (default: all)"
	@echo "  TIMING   - Timing mode: ns, cycle (default: ns)"
	@echo "  USE_KLIB - Use klib instead of standard library: yes, no (default: no)"
	@echo ""
	@echo "Library modes:"
	@echo "  USE_KLIB=no  - Use standard library headers (stdio.h, stdint.h, etc.)"
	@echo "  USE_KLIB=yes - Use klib.h header instead of standard library"
	@echo ""
	@echo "Examples:"
	@echo "  make                                    # Build combined tests for all architectures (std lib)"
	@echo "  make ARCH=riscv TIMING=cycle            # Build combined RISC-V tests with cycle timing"
	@echo "  make separate ARCH=x86 USE_KLIB=yes     # Build separate x86 tests with klib"
	@echo "  make USE_KLIB=yes TIMING=cycle          # Build combined tests with klib and cycle timing"
	@echo "  make clean                              # Clean all files"
