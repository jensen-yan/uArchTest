# InstructionRate/Makefile
.PHONY: default
default: all ;

CC := gcc
# Cross-compilers
CROSS_COMPILE_AARCH64 := aarch64-linux-gnu-
CROSS_COMPILE_X86_64 := x86_64-linux-gnu-

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
	@${CROSS_COMPILE_X86_64}${CC} ${CFLAGS} x86instructionrate.s x86instructionrate.c x86instructionrate.h -o InstructionRate_x86 ${LDFLAGS}
	@echo "----------------------------------------"

# Rule to compile the generated lines for arm architecture
compile_arm:
	@echo "----------------------------------------"
	@echo "Compiling for ARM architecture..."
	@${CROSS_COMPILE_AARCH64}${CC} ${CFLAGS} arminstructionrate.s arminstructionrate.c arminstructionrate.h -o InstructionRate_arm ${LDFLAGS}
	@echo "----------------------------------------"

# Default target
all: compile_x86 compile_arm

# Clean rule to remove generated files, binaries, and the output directory
clean:
	@echo "----------------------------------------"
	@echo "Cleaning up all generated files and directories..."
	@rm -rf InstructionRate_x86 InstructionRate_arm
	@echo "----------------------------------------"

