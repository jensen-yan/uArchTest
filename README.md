# uArchTest

uArchTest is a collection of CPU microarchitecture benchmarks derived from
[Chips and Cheese Microbenchmarks](https://github.com/ChipsandCheese/Microbenchmarks).
It uses C plus handwritten or generated assembly to probe frontend, execution,
memory, clock, and coherency behavior.

## Components

- `AsmGen`: .NET-based generator for combined or per-test assembly benchmarks.
  It covers ROB, LDQ/STQ, register files, schedulers, BTB, branch history,
  indirect branches, return stack, throughput, and latency chains.
- `MemoryLatency`: cache/TLB/MLP/store-to-load-forwarding latency tests.
- `MemoryBandwidth`: data and instruction bandwidth tests, including threaded
  read/write/copy modes.
- `InstructionRate`: ISA instruction throughput and latency tests.
- `CoreClockChecker`: per-core clock and optional MSR/RAPL power checks.
- `CoherencyLatency`: pthread-based core-to-core cache-line handoff latency.

## Prerequisites

- `make`
- x86 compiler: `x86_64-linux-gnu-gcc`
- optional ARM compiler: `aarch64-linux-gnu-gcc`
- optional RISC-V compiler: `riscv64-unknown-linux-gnu-gcc`
- optional .NET SDK 8 for `AsmGen`

If the RISC-V compiler is configured only in `~/.zshrc`, run the build from zsh
or export the same PATH first. To build `AsmGen`, install .NET SDK 8; with Nix,
`nix-env -iA nixpkgs.dotnet-sdk` is sufficient.

## Build

Build the x86 benchmark set:

```bash
make
# same as:
make compile_x86
```

If `dotnet` is not installed, the top-level build skips `AsmGen` and still builds
the C/assembly subprojects.

Build RISC-V-supported benchmarks:

```bash
make compile_riscv
```

Build a single subproject:

```bash
make -C MemoryLatency compile_x86
make -C MemoryLatency compile_riscv
make -C AsmGen ARCH=x86
```

For `AsmGen` RISC-V experiments in gem5, start with one generated test instead
of the full combined benchmark. The full RISC-V assembly file is very large and
slow to compile.

```bash
make -C AsmGen clean
make -C AsmGen ARCH=riscv TIMING=cycle ONLY=nopbw
```

Then run the generated binary with gem5 SE, for example:

```bash
/path/to/gem5.opt -d /tmp/uarchtest-nopbw \
  /path/to/configs/example/se.py \
  --no-pf -I 5000000 \
  --cmd=$PWD/AsmGen/generate/clammicrobench_riscv \
  --options="nopbw 10"
```

Clean generated binaries:

```bash
make clean
```

## Quick Smoke Tests

These commands are intentionally tiny and should finish quickly:

```bash
./MemoryLatency/MemoryLatency_x86 -test c -sizekb 2 -iter 1000
./MemoryLatency/MemoryLatency_x86 -test asm -sizekb 2 -iter 1000
./MemoryBandwidth/MemoryBandwidth_x86 -threads 1 -private -sizekb 2 -data 1 -method scalar
```

Some tools run long by default. Use small iteration counts first, especially for
`InstructionRate`, `CoreClockChecker`, and all-core coherency tests.

## License

This project is licensed under Apache-2.0. See `LICENSE`.
