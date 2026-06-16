# AsmGen gem5 SE ideal_kmhv3 capacity probe

测试配置：

- gem5: `/nfs/home/yanyue/workspace/GEM5_4/build/RISCV/gem5.opt`
- 配置入口：`configs/example/se.py --ideal-kmhv3`
- Prefetch: 默认打开，没有传 `--no-pf`
- AsmGen: RISC-V `TIMING=cycle`
- Runner: `tools/run_gem5_asmgen_capacity.sh`
- `ITER=10`, `MAXINSTS=0`, `TIMEOUT=300s`

这轮先按源码参数选候选窗口，而不是跑全量：

| 测试 | 窗口 | 源码预期 |
| --- | --- | --- |
| `rob` | 240..400 step 4 | `numROBEntries=160`, `CROB_instPerGroup=2`，约 320 inst |
| `ldq` | 96..160 step 2 | `LQEntries=128` |
| `stq` | 48..88 step 2 | `SQEntries=64` |
| `addsched` | 64..128 step 2 | 6 个 int IQ，每个 16 项，总量约 96 |
| `mulsched` | 20..56 step 2 | 2 个带 `IntMult` 的 int IQ，每个 16 项，总量约 32 |
| `loadsched` | 32..72 step 2 | 3 个 load IQ，每个 16 项，总量约 48 |
| `storeaddrsched` | 20..56 step 2 | 2 个 store-address IQ，每个 16 项，总量约 32 |
| `storedatasched` | 20..56 step 2 | 2 个 store-data IQ，每个 16 项，总量约 32 |

## 结果总览

| 测试 | 点数 | baseline(first8) | 首个 >= 1.5x | 最大点 | 结论 |
| --- | ---: | ---: | ---: | --- | --- |
| `rob` | 41 | 47.05 | 400 | 400: 72.60 | 平滑上升，无 320 硬台阶 |
| `ldq` | 33 | 56.64 | - | 160: 76.20 | 96..160 内无 128 硬台阶 |
| `stq` | 21 | 50.65 | - | 88: 67.10 | 48..88 内无 64 硬台阶 |
| `addsched` | 33 | 45.48 | - | 128: 59.20 | 64..128 内无 96 硬台阶 |
| `mulsched` | 19 | 45.76 | - | 56: 66.20 | 20..56 内无 32 硬台阶 |
| `loadsched` | 21 | 44.00 | - | 68/72: 57.30 | 32..72 内无 48 硬台阶 |
| `storeaddrsched` | 19 | 42.48 | - | 56: 55.30 | 20..56 内无 32 硬台阶 |
| `storedatasched` | 19 | 41.19 | - | 56: 55.00 | 20..56 内无 32 硬台阶 |

## 怎么读

这轮最重要的结论不是“容量都没到”，而是当前 AsmGen 这些结构测试在 gem5 ideal 上多数呈现为平滑斜坡。它们输出的是 `cycles/iteration`，而每个点的循环体指令数随 `x` 增长，所以原始数值天然会随 `x` 线性上升。要找容量 cliff，不能只看第一组点的 `1.5x` 阈值。

`rob` 在 320 附近没有跳变：`316=57.3`, `320=58.0`, `324=58.6`, `328=60.6`，更像 NOP filler 长度增加后的窗口/前端/执行代价曲线。它不适合直接读作 “gem5 ROB 容量不是 320”。

`ldq/stq` 也没有在 `128/64` 附近形成硬台阶。`ldq` 从 `96=51.7` 平滑涨到 `160=76.2`；`stq` 从 `48=46.5` 平滑涨到 `88=67.1`。这和 x86 上 `ldq/stq` 的明显台阶不同，说明当前 RISC-V/gem5 路径下这个测试形状没有把 LQ/SQ full 暴露成单点 cliff。

`addsched/mulsched/loadsched/store*` 都更像吞吐斜率测试。源码容量点附近没有明显非线性，例如 `loadsched` 在 48 附近只是 `46=46.8`, `48=49.3`, `50=48.9`。

## 本轮修复

- 给 AsmGen 增加了 `--range low:high[:step]` / `--range prefix=low:high[:step]`，避免容量类全量盲跑。
- Makefile 增加 `RANGE=` 透传。
- 修复 `LoadSchedTest` 的 RISC-V 第二组 load，把误写的 ARM `ldr` 改成 RISC-V `ld`。
- 新增 `tools/run_gem5_asmgen_capacity.sh`，默认按源码候选范围跑容量类。

## 下一步建议

下一轮不要简单提高 `ITER` 重复这批窗口。更有价值的是把 stats 和每个 `x` 关联起来：

- 方案 A：一次只生成/运行一个 `x`，解析 `stats.txt` 里的 `system.cpu.iew.iqFullEvents`, `system.cpu.iew.lsqFullEvents`, `system.cpu.iew.stallEvents::*`, `system.cpu.rename.stallEvents::*`。
- 方案 B：在 AsmGen 每个点前后插入 gem5 m5 dump/reset stats hook，这样一个进程内也能分段统计。
- 方案 C：针对每类资源重写更干净的 probe，例如 ROB 用非 NOP 混合 uop filler，IQ 用能稳定进入目标 issue queue 的独立 uop，并对线性基线做 residual 分析。

当前结论先写成：这轮验证了候选窗口和运行链路，但 AsmGen 原始 capacity 输出在 gem5 ideal 上没有直接给出清晰容量 cliff。
