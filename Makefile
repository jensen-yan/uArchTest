# List of subdirectories
SUBDIRS := AsmGen CoherencyLatency CoreClockChecker InstructionRate MemoryBandwidth MemoryLatency

CC := gcc
# Cross-compilers
CROSS_COMPILE_AARCH64 := aarch64-linux-gnu-
CROSS_COMPILE_X86_64 := x86_64-linux-gnu-

# Compiler flags
CFLAGS := -O2

# Linker flags
LDFLAGS := -static -pthread -lm

.PHONY: default all clean $(SUBDIRS)

default: all

# Rule to check if x86-64 cross-compiler is installed
check-x86-64-compiler:
	@if ! command -v ${CROSS_COMPILE_X86_64}${CC} > /dev/null; then \
		echo "x86-64 cross-compiler is not installed"; \
		exit 1; \
	fi
	
# Rule to check if aarch64 cross-compiler is installed
check-aarch64-compiler:
	@if ! command -v ${CROSS_COMPILE_AARCH64}${CC} > /dev/null; then \
		echo "aarch64 cross-compiler is not installed"; \
		exit 1; \
	fi

# Rule to compile subdirectories for x86
compile_x86: check-x86-64-compiler
	@$(MAKE) -j $(SUBDIRS) -C $@ compile_x86
	@$(MAKE) copy_x86

# Rule to compile subdirectories for ARM
compile_arm: check-aarch64-compiler
	@$(MAKE) -j $(SUBDIRS) -C $@ compile_arm
	@$(MAKE) copy_arm

copy_x86:
	@mkdir -p uArchBin_x86
	@find . -path ./uArchBin_x86 -prune -o -name "*_x86" -exec cp {} uArchBin_x86 \;

copy_arm:
	@mkdir -p uArchBin_arm
	@find . -path ./uArchBin_arm -prune -o -name "*_arm" -exec cp {} uArchBin_arm \;

copy: copy_x86 copy_arm;


# Clean rule for subdirectories
clean:
	@for dir in $(SUBDIRS); do \
		$(MAKE) -C $$dir clean; \
	done
	@rm -rf uArchBin_x86 uArchBin_arm

# All rule to build all subdirectories
all: $(SUBDIRS) 
	@$(MAKE) copy

# Implicit rule to build each subdirectory
$(SUBDIRS):
	$(MAKE) -C $@
