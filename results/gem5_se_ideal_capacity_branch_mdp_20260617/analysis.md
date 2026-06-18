# GEM5 SE ideal_kmhv3 AsmGen branch/MDP 探针

本轮补跑 branch queue、jump scheduler、MDP 和 return stack 相关测试，prefetch 保持默认开启。

- Host: `node037.bosccluster.com`
- GEM5: `/nfs/home/yanyue/workspace/GEM5_4`
- GEM5 branch: `codex/se-aligned-l2-mip-diff`
- GEM5 commit: `f1f870736b42`
- Command wrapper: `ITER=10 MAXINSTS=0 TIMEOUT=300s tools/run_gem5_asmgen_capacity.sh jmpsched ftq brq mdp returnstack`
- Output: `results/gem5_se_ideal_capacity_branch_mdp_20260617/summary.tsv`

注意：这个批次运行时 GEM5 commit 已经变成 `f1f870736b42`，和前面的基本组、RF/FP 组不是同一个 commit。这里主要看测试形状，不做严格性能横比。

## 自动摘要

| 测试 | 范围 | baseline(first8) | first >= 1.5x | first >= 2.0x | max x | max cycles | 状态 |
| --- | --- | ---: | --- | --- | ---: | ---: | --- |
| `jmpsched` | `32:96:2` | 55.225 | 72 | NA | 80 | 91.700 | 完成，但非单调 |
| `ftq` | `32:96:2` | 114.625 | 56 | 74 | 96 | 302.700 | 完成，线性变重 |
| `brq` | `32:96:2` | 44.350 | NA | NA | 96 | 60.500 | 完成，平滑上升 |
| `mdp` | `32:160:4` | 1.750161 | NA | NA | 132 | 1.753758 | 完成，几乎恒定 |
| `returnstack` | `1:96:1` | NA | NA | NA | NA | NA | timeout，status=124 |

## 逐项解读

`jmpsched` 在 `x=32..80` 大体上升，但 `x=82` 又从 `91.7` 掉到 `67.0`，后续维持在 `67..72`。这个形状更像代码布局/预测/取指行为混在一起，不像一个可读的 scheduler 容量断点。

`ftq` 从 `x=32` 的 `59.1` 快速涨到 `x=96` 的 `302.7`，自动规则给出 `56/74`，但曲线从很早就线性变重，没有在源码配置的 `ftq_size=64` 附近形成硬台阶。它说明 taken branch 压力会显著拉高成本，但这版测试不能直接读 FTQ entry 数。

`brq` 从 `40.5` 平滑涨到 `60.5`，没有容量台阶。

`mdp` 在 `x=32..160` 基本稳定在 `1.750 cycles` 左右，说明这个范围内没有触发明显 memory-dependence predictor 压力。它不是坏结果，反而说明当前探针没有命中 MDP 容量/冲突路径。

`returnstack` 在 `1:96:1, ITER=10` 下被 `300s` timeout 杀掉，未打印任何有效点。后续要测 return stack，应单独降范围或改 runner，例如先跑 `1:48:1`、`ITER=1`，再围绕可疑点复测。

## 当前结论

branch/MDP 这批也不能直接给结构容量数字。最有用的现象是：

- `mdp` 在当前范围内基本无压力信号。
- `ftq` 对 taken branch count 很敏感，但不是 64 附近硬台阶。
- `returnstack` 需要单独小范围跑，不能混在批量 capacity runner 里。

所以到目前为止，GEM5 AsmGen 里最可信的仍然是基本吞吐/延迟和 `ldq/stq`；ROB、scheduler、RF、branch queue 这些需要改测试形状或按单点 stats 重做。
