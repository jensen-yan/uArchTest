# GEM5 SE ideal_kmhv3 AsmGen RF/FP 容量扫

本轮补跑 RF 和 FP scheduler 相关测试，prefetch 保持默认开启。

- Host: `node037.bosccluster.com`
- GEM5: `/nfs/home/yanyue/workspace/GEM5_4`
- GEM5 branch: `codex/se-aligned-l2-mip-diff`
- GEM5 commit: `a105ca0fae38`
- Command wrapper: `ITER=10 MAXINSTS=0 TIMEOUT=300s tools/run_gem5_asmgen_capacity.sh intrf fprf faddsched fmulsched`
- Output: `results/gem5_se_ideal_capacity_rf_fp_20260617/summary.tsv`

## 自动摘要

| 测试 | 范围 | baseline(first8) | first >= 1.5x | first >= 2.0x | max x | max cycles/iter | 读法 |
| --- | --- | ---: | --- | --- | ---: | ---: | --- |
| `intrf` | `128:256:4` | 63.613 | NA | NA | 256 | 92.500 | 平滑上升，无硬台阶 |
| `fprf` | `160:320:4` | 90.000 | 264 | NA | 320 | 163.000 | 阈值点像线性穿越，不像容量断点 |
| `faddsched` | `40:112:2` | 28.138 | 78 | 106 | 112 | 60.100 | 近似线性增长，不宜读成 scheduler 容量 |
| `fmulsched` | `40:112:2` | 52.000 | 74 | 98 | 112 | 115.900 | 近似线性增长，不宜读成 scheduler 容量 |

## 曲线形状

`intrf` 从 `x=128` 的 `59.5 cycles/iter` 平滑涨到 `x=256` 的 `92.5 cycles/iter`，没有在 `numPhysIntRegs=224` 附近出现台阶。

`fprf` 从 `x=160` 的 `83 cycles/iter` 基本每增加 4 个 count 就涨约 2 cycles，到 `x=320` 是 `163 cycles/iter`。自动规则给出的 `x=264` 只是越过 1.5x 阈值的位置，不应解释成 FP RF 容量。

`faddsched/fmulsched` 更像在测每个额外 FP 操作的线性执行成本：

- `faddsched`: `40 -> 112` 约从 `24.1` 涨到 `60.1`
- `fmulsched`: `40 -> 112` 约从 `44.0` 涨到 `115.9`

两者都没有“容量耗尽后突然变慢”的断点。

## 当前结论

这轮 RF/FP 测试没有给出可信容量点。它们更像“随着 count 增加，循环体指令数线性增加”的测量，而不是结构容量耗尽。

这和整数 scheduler 那批结果一致：当前 RISC-V AsmGen structure helper 还没有把 scheduler/RF 类资源压成清晰台阶。后续如果要继续追，应该先把测试改成 single-point stats，并让 RISC-V helper 和 x86 helper 的双段 filler 形状对齐。
