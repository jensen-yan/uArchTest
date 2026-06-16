SUBDIRS := AsmGen CoherencyLatency CoreClockChecker InstructionRate MemoryBandwidth MemoryLatency
RISCV_SUBDIRS := AsmGen MemoryLatency

CC := gcc
CROSS_COMPILE_AARCH64 ?= aarch64-linux-gnu-
CROSS_COMPILE_X86_64 ?= x86_64-linux-gnu-
CROSS_COMPILE_RISCV64 ?= riscv64-unknown-linux-gnu-
RISCV_TOOLCHAIN ?=

DOTNET := $(shell command -v dotnet 2>/dev/null)
ifeq ($(DOTNET),)
BUILD_SUBDIRS := $(filter-out AsmGen,$(SUBDIRS))
BUILD_RISCV_SUBDIRS := $(filter-out AsmGen,$(RISCV_SUBDIRS))
SKIP_ASMGEN := yes
else
BUILD_SUBDIRS := $(SUBDIRS)
BUILD_RISCV_SUBDIRS := $(RISCV_SUBDIRS)
SKIP_ASMGEN := no
endif

CFLAGS := -O2
LDFLAGS := -static -pthread -lm

.PHONY: default all compile_all_arch compile_x86 compile_arm compile_riscv \
	check-x86-64-compiler check-aarch64-compiler check-riscv64-compiler \
	copy copy_x86 copy_arm copy_riscv clean $(SUBDIRS)

default: all

check-x86-64-compiler:
	@if ! command -v ${CROSS_COMPILE_X86_64}${CC} > /dev/null; then \
		echo "x86-64 cross-compiler is not installed"; \
		exit 1; \
	fi
	
check-aarch64-compiler:
	@if ! command -v ${CROSS_COMPILE_AARCH64}${CC} > /dev/null; then \
		echo "aarch64 cross-compiler is not installed"; \
		exit 1; \
	fi

check-riscv64-compiler:
	@if [ -n "$(RISCV_TOOLCHAIN)" ]; then \
		export PATH="$(RISCV_TOOLCHAIN):$$PATH"; \
	fi; \
	if ! command -v ${CROSS_COMPILE_RISCV64}${CC} > /dev/null; then \
		echo "riscv64 cross-compiler is not installed"; \
		echo "Set RISCV_TOOLCHAIN or CROSS_COMPILE_RISCV64 if it is installed in a non-standard path."; \
		exit 1; \
	fi

all: compile_x86

compile_all_arch: compile_x86 compile_arm compile_riscv

compile_x86: check-x86-64-compiler
	@if [ "$(SKIP_ASMGEN)" = "yes" ]; then \
		echo "Skipping AsmGen: dotnet CLI is not installed"; \
	fi
	@for dir in $(BUILD_SUBDIRS); do \
		echo "==> $$dir: compile_x86"; \
		$(MAKE) -C $$dir compile_x86 || exit $$?; \
	done
	@$(MAKE) copy_x86

compile_arm: check-aarch64-compiler
	@if [ "$(SKIP_ASMGEN)" = "yes" ]; then \
		echo "Skipping AsmGen: dotnet CLI is not installed"; \
	fi
	@for dir in $(BUILD_SUBDIRS); do \
		echo "==> $$dir: compile_arm"; \
		$(MAKE) -C $$dir compile_arm || exit $$?; \
	done
	@$(MAKE) copy_arm

compile_riscv: check-riscv64-compiler
	@if [ "$(SKIP_ASMGEN)" = "yes" ]; then \
		echo "Skipping AsmGen: dotnet CLI is not installed"; \
	fi
	@for dir in $(BUILD_RISCV_SUBDIRS); do \
		echo "==> $$dir: compile_riscv"; \
		if [ -n "$(RISCV_TOOLCHAIN)" ]; then \
			PATH="$(RISCV_TOOLCHAIN):$$PATH" $(MAKE) -C $$dir compile_riscv || exit $$?; \
		else \
			$(MAKE) -C $$dir compile_riscv || exit $$?; \
		fi; \
	done
	@$(MAKE) copy_riscv

copy_x86:
	@mkdir -p uArchBin_x86
	@find . -path ./uArchBin_x86 -prune -o -name "*_x86" -exec cp {} uArchBin_x86 \;

copy_arm:
	@mkdir -p uArchBin_arm
	@find . -path ./uArchBin_arm -prune -o -name "*_arm" -exec cp {} uArchBin_arm \;

copy_riscv:
	@mkdir -p uArchBin_riscv
	@find . -path ./uArchBin_riscv -prune -o -name "*_riscv" -exec cp {} uArchBin_riscv \;

copy: copy_x86 copy_arm copy_riscv

clean:
	@for dir in $(SUBDIRS); do \
		$(MAKE) -C $$dir clean; \
	done
	@rm -rf uArchBin_x86 uArchBin_arm uArchBin_riscv

$(SUBDIRS):
	$(MAKE) -C $@
