# GEM5 SE ideal_kmhv3 AsmGen 单点容量测试

这轮是对前面容量测试结论的修正。批量 runner 一次扫完整个 range，`cycles/iteration`
只能看到平滑的总体成本，gem5 stats 也是整轮聚合值；用它判断 ROB/scheduler/RF
容量很容易误判。新的 `tools/run_gem5_asmgen_points.sh` 改成每个 `x` 单独
build/run，再读该点的 stats。

- Host: `node037.bosccluster.com`
- GEM5: `/nfs/home/yanyue/workspace/GEM5_4`
- GEM5 branch: `codex/se-aligned-l2-mip-diff`
- GEM5 commit: `f1f870736b42`
- Config: `configs/example/se.py --ideal-kmhv3`
- Prefetch: 默认开启
- 主要信号：scheduler 类看 `system.cpu.iew.iqFullEvents`；ROB/RF 类看 rename full events

## 为什么之前看起来都失败

不是容量结构都真的测不到，而是前一版读法不对：

1. `cycles/iteration` 对这些小 probe 太平滑，额外指令数、循环边界、fetch/rename/commit
   固定成本会把容量断点冲淡。
2. `iqFullEvents` 有背景噪声，所以不能用 first nonzero 当容量点；应该看相对低
   x baseline 的突增。
3. 批量扫 range 时 stats 没法归因到某个 x。单点 runner 解决了这个问题。
4. 部分 AsmGen 测试形状本身不适合 GEM5：例如 `rob` 用 NOP 填窗口，而 GEM5 O3
   对 NOP 有特殊处理；RF 测试也没有触发 `fullRegistersEvents`。

## 已经测出的结构

| 测试 | 源码候选 | 单点 stats 现象 | 当前读数 |
| --- | ---: | --- | --- |
| `addsched` | 6 x 16 = 96 IntALU IQ entries | `iqFullEvents` 从 x=91 开始约 2x，x=92 约 5x，x=95 约 10x | 有效阈值约 92-96 |
| `mulsched` | 2 x 16 = 32 IntMult-capable IQ entries | x=30/31 约 3.3k，x=32 约 4.6k，x=33 跳到约 48.7k | 约 32/33 |
| `loadsched` | 3 x 16 = 48 load IQ entries | 加 divide blocker 后，x=40/44 约 3.1k/3.4k，x=48 跳到约 16.2k | 约 48 |
| `storeaddrsched` | 2 x 16 = 32 store-address IQ entries | 加 divide blocker 后，x=24/28/32 约 3.1k/3.6k/3.7k，x=36 跳到约 17.1k | 约 32-36 |
| `storedatasched` | 2 x 16 = 32 store-data IQ entries | x=30-32 约 3.3k-3.6k，x=33 跳到约 46.4k | 约 32/33 |

这些结果和 `ideal_kmhv3` 源码配置基本对齐。`addsched` 比 96 略早出现压力也合理：
循环控制、辅助指令和实际 issue/dispatch 时序都会占掉一点窗口空间，所以它更像“有效阈值”
而不是裸结构 entry 数。

## 仍然没有测准的结构

| 测试 | 源码候选 | 单点结果 | 判断 |
| --- | ---: | --- | --- |
| `rob` | `numROBEntries=160`, `CROB_instPerGroup=2` | NOP 版 x=120 到 440 为 0；`add x0` + 12-div blocker 版 x=240 到 440 仍为 0 | 当前 ROB probe 仍无效 |
| `intrf` | `numPhysIntRegs=224` | WAW filler + 8-div blocker 到 x=240 为 0；把全部 IQ 临时拉到 256 后扫到 x=416 仍为 0 | 当前 RF blocker 仍不够，不能读容量 |
| `fprf` | `numPhysFloatRegs=256` | WAW filler + 4-div blocker 到 x=288 为 0；大 IQ 后扫到 x=480 仍为 0 | 当前 FP RF probe 仍不够，不能读容量 |

所以“容量结构类都失败”这个说法要拆开看：整数 ALU、乘法、store-data scheduler
和 load/store-address scheduler 已经能读出可信容量；ROB、RF 还需要更专门的测试形状。

## 关键数据

`results/gem5_se_ideal_points_sched_fine_20260617/summary.tsv`：

| 测试 | baseline(first2) | first >= 2x | first >= 5x | first >= 10x |
| --- | ---: | --- | --- | --- |
| `addsched` | 4516.5 | 91 | 92 | 95 |
| `mulsched` | 3312.0 | 33 | 33 | 33 |
| `storedatasched` | 3414.5 | 33 | 33 | 33 |

`results/gem5_se_ideal_points_blocked_fix_20260617/summary.tsv`：

| 测试 | baseline(first2) | first >= 2x | first >= 5x | first >= 10x |
| --- | ---: | --- | --- | --- |
| `loadsched` | 3223.5 | 48 | 48 | NA |
| `storeaddrsched` | 3393.5 | 36 | 36 | NA |

RF/ROB 负结果：

- `rob=240:440:40`，12-div blocker，`ROBFullEvents` 一直是 0。
- `intrf=224:416:32`，全部 IQ 临时设为 256，`fullRegistersEvents` 一直是 0。
- `fprf=256:480:32`，全部 IQ 临时设为 256，`fullRegistersEvents` 一直是 0。

## 下一步修测试

1. `rob`：`add x0` 也不是可靠 filler。下一版应改成不会被 NOP/zero-dest 特殊处理、
   同时不先打满 IQ/SQ/RF 的 ROB-only filler；必要时要在 GEM5 里加一个专门的 probe op
   或统计窗口占用来验证。
2. `intrf/fprf`：现在 WAW filler 已经避免了 RAW 链，但提交阻塞仍不够强；下一版更适合
   用 cache-miss/uncached older load 或跨类别长延迟 blocker，而不是让同类 IQ 先成为瓶颈。
3. `loadsched/storeaddrsched`：当前 RISC-V blocked pattern 已经能读出容量，后续只需要围绕
   断点做 `step=1` 复扫即可。
4. 继续使用单点 runner，而不是批量 range runner，读容量时看 stats 跃迁而不是只看
   `cycles/iteration`。
