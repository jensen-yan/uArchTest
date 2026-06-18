# GEM5 SE ideal_kmhv3 AsmGen 基本吞吐/延迟

本轮用 `se.py --ideal-kmhv3` 跑 AsmGen 基本测试，prefetch 保持默认开启。

- Host: `node037.bosccluster.com`
- GEM5: `/nfs/home/yanyue/workspace/GEM5_4`
- GEM5 branch: `codex/se-aligned-l2-mip-diff`
- GEM5 commit: `a105ca0fae38`
- Command wrapper: `ITER=10 TIMEOUT=120s tools/run_gem5_asmgen_se.sh nopbw addbw mulbw loadbw addlat mullat loadlat`
- Output: `results/gem5_se_ideal_basic_pf_iter10_current_20260617/summary.tsv`

注意：补跑时 `/nfs/home/yanyue/workspace/GEM5_4` 工作区有本地修改，所以这份结果适合看当前现场的基本行为；如果要做公开对比，最好固定 clean commit 后再重跑。

## 结果表

`reported_cycles_per_op` 是 AsmGen 用 `rdcycle` 得到的每条被测操作平均 cycle。`op_per_cycle = 1 / cycles_per_op`。

| 测试 | GEM5 cycles/op | GEM5 op/cycle | x86 Zen 4 cycles/op | x86 op/cycle | 读法 |
| --- | ---: | ---: | ---: | ---: | --- |
| `nopbw` | 0.167072 | 5.99 | 0.085 | 11.7 | GEM5 约 6 NOP/cycle，低于 x86 的 NOP 特例上限 |
| `addbw` | 0.167072 | 5.99 | 0.254 | 3.94 | GEM5 约 6 ADD/cycle，高于这台 Zen 4 的约 4 ADD/cycle |
| `mulbw` | 0.500055 | 2.00 | 1.01 | 0.99 | GEM5 约 2 MUL/cycle，x86 约 1 MUL/cycle |
| `loadbw` | 0.333391 | 3.00 | 0.339 | 2.95 | 两边都接近 3 load/cycle |
| `addlat` | 1.000035 | - | 1.01 | - | 依赖 add 链延迟约 1 cycle |
| `mullat` | 3.000037 | - | 3.03 | - | 依赖 mul 链延迟约 3 cycles |
| `loadlat` | 4.000068 | - | 4.04 | - | 依赖 L1 load-use 延迟约 4 cycles |

## 结论

这组基本吞吐/延迟和 `ideal_kmhv3` 源码配置很一致：

- `nopbw/addbw ~= 6 op/cycle`：`ideal_kmhv3` 配置里 decode/rename 是 8 宽、commit 是 12 宽，KMHV3 scheduler 有 6 个 integer IQ/IntALU 发射口，所以普通整数 add 可以跑到约 6/cycle。
- `mulbw ~= 2 op/cycle`：KMHV3 里有 2 个带 `IntMult` 的 integer issue queue，测出来也是 2/cycle。
- `loadbw ~= 3 op/cycle`：KMHV3 有 3 个 load issue queue，测出来也是 3/cycle。
- `addlat/mullat/loadlat = 1/3/4 cycles`：这些 latency 数字和 x86 Zen 4 非常接近，尤其 `loadlat` 的 4 cycles 是一个很好的 sanity check。

所以当前看，GEM5 的基本执行侧模型不是“整体偏慢”；它更像是按 `ideal_kmhv3` 的理想端口数量给出了干净的峰值。和 x86 的差异主要是端口配置差异：x86 NOP 有特殊处理，ADD/MUL 端口数也不同；load 侧反而几乎重合。

## 额外说明

早一轮 `ITER=100` 在另一个 GEM5 commit `c6486def9e91` 上跑过前六项，结果和本轮基本一致：

- `nopbw/addbw`: `0.167 cycles/op`
- `mulbw`: `0.500 cycles/op`
- `loadbw`: `0.333 cycles/op`
- `addlat`: `1.000 cycles`
- `mullat`: `3.000 cycles`

那轮 `loadlat ITER=100` 被 `180s` timeout 杀掉，没有有效 report 点；本轮用 `ITER=10` 补跑后得到 `4.000 cycles`。
