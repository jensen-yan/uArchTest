# MemoryBandwidth/Makefile
.PHONY: default
default: all ;

CC := gcc
# Cross-compilers
CROSS_COMPILE_AARCH64 := aarch64-linux-gnu-
CROSS_COMPILE_X86_64 := x86_64-linux-gnu-
CROSS_COMPILE_RISCV64 := riscv64-unknown-linux-gnu-
# Compiler flags
CFLAGS := 
CFLAGS += -O2

# Linker flags
LDFLAGS := 
LDFLAGS += -static -pthread -lm

# Rule to compile the generated files for x86 architecture
compile_x86:
	@echo "----------------------------------------"
	@echo "Compiling for x86 architecture..."
	@${CROSS_COMPILE_X86_64}${CC} ${CFLAGS} MemoryLatency.c MemoryLatency_x86.s -o MemoryLatency_x86 ${LDFLAGS}
	@echo "----------------------------------------"

# Rule to compile the generated lines for arm architecture
compile_arm:
	@echo "----------------------------------------"
	@echo "Compiling for ARM architecture..."
	@${CROSS_COMPILE_AARCH64}${CC} ${CFLAGS} MemoryLatency.c MemoryLatency_arm.s -o MemoryLatency_arm ${LDFLAGS}
	@echo "----------------------------------------"

# Rule to compile the generated lines for riscv architecture
compile_riscv:
	@echo "----------------------------------------"
	@echo "Compiling for RISCV architecture..."
	@${CROSS_COMPILE_RISCV64}${CC} ${CFLAGS} MemoryLatency.c MemoryLatency_riscv.s -o MemoryLatency_riscv ${LDFLAGS}
	@echo "----------------------------------------"

# Default target
all: compile_x86 compile_arm compile_riscv

# Clean rule to remove generated files, binaries, and the output directory
clean:
	@echo "----------------------------------------"
	@echo "Cleaning up all generated files and directories..."
	@rm -rf MemoryLatency_x86 MemoryLatency_arm MemoryLatency_riscv
	@echo "----------------------------------------"

