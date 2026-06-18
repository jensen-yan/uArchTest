# GEM5 SE ideal_kmhv3 AsmGen 非 LDQ/STQ 容量扫

更新：这份文档是早期批量 range runner 的结论，后来已被单点 stats runner 部分修正。
新的结论见 `results/gem5_se_ideal_capacity_points_20260617/analysis.md`：
`addsched/mulsched/storedatasched` 可以从 `iqFullEvents` 跃迁读出接近源码配置的容量；
`loadsched/storeaddrsched` 在加入 RISC-V divide blocker 后也能读出容量；
`rob/rf` 仍需要改测试形状。

本轮跳过纯 `ldq/stq`，继续扫 AsmGen 里的非 LDQ/STQ 容量类测试。

- Host: `node037.bosccluster.com`
- GEM5: `/nfs/home/yanyue/workspace/GEM5_4`
- GEM5 branch: `codex/riscv-fflags-exe-helper`
- GEM5 commit: `c6486def9e91`
- Command wrapper: `ITER=10 MAXINSTS=0 TIMEOUT=300s tools/run_gem5_asmgen_capacity.sh rob addsched mulsched loadsched storeaddrsched storedatasched mixloadstoresched`
- Output: `results/gem5_se_ideal_capacity_fencei_nonldst_20260617/summary.tsv`
- Prefetch: 默认开启

注意：这组容量扫和 `results/gem5_se_ideal_basic_pf_iter10_current_20260617` 不是同一个 GEM5 commit，不能做严格横向性能对比。

## 自动摘要

runner 的自动规则是用前 8 个点均值做 baseline，然后找第一个超过 `1.5x/2.0x` 的点。这一轮所有非 LDQ/STQ 项都没有超过 `1.5x`，说明没有观察到像 `ldq/stq` 那样的硬台阶。

| 测试 | 范围 | baseline(first8) | max x | max cycles/iter | first >= 1.5x | 读法 |
| --- | --- | ---: | ---: | ---: | --- | --- |
| `rob` | `240:400:4` | 70.475 | 400 | 94.500 | NA | 平滑上升，不能判断 ROB 容量 |
| `addsched` | `64:128:2` | 54.688 | 128 | 69.600 | NA | 无硬台阶 |
| `mulsched` | `20:56:2` | 59.750 | 56 | 81.200 | NA | 无硬台阶 |
| `loadsched` | `32:72:2` | 60.825 | 68 | 72.700 | NA | 无硬台阶 |
| `storeaddrsched` | `20:56:2` | 55.525 | 56 | 69.700 | NA | 无硬台阶 |
| `storedatasched` | `20:56:2` | 54.575 | 56 | 70.000 | NA | 无硬台阶 |
| `mixloadstoresched` | `32:96:2` | 57.413 | 92 | 71.600 | NA | 无硬台阶 |

## 当前结论

这轮不应该直接给出 `addsched/mulsched/loadsched/store*` 的容量数字。它们的 cycles/iteration 都随 count 缓慢增加，但没有像 x86 `loadsched` 或 GEM5 `ldq/stq` 那样出现明显断点。

我更倾向于把它解释为“当前 RISC-V AsmGen structure 测试形状还不够适合测这些 scheduler 容量”，原因有三点：

1. `rob` 本身就是低置信度测试。它用 NOP 填充窗口，之前在 x86 上也证明不能直接拿来读 ROB entry 数；GEM5 这里同样只是平滑上升。
2. RISC-V structure helper 和 x86 版不完全同构。当前 RISC-V helper 只展开 `fillerInstrs1`，没有真正使用 `fillerInstrs2` 形成第二段 filler；这会影响 scheduler 类测试的压力形状。
3. 当前 runner 每个测试只生成一个二进制并连续扫全部 x，gem5 stats 是整轮聚合值，无法把某个 x 点和具体 `IQ full` / `arbFailed` / queue occupancy 对上。对容量类来说，仅靠最终 cycles/iteration 容易把“固定边界成本 + 指令数线性成本”误读成容量现象。

## 和源码预期的关系

`ideal_kmhv3` 的资源配置大致是：

- 6 个 integer issue queue，每个 size 16，且 6 个都有 `IntALU`。
- 2 个 integer issue queue 带 `IntMult`。
- 3 个 load issue queue，每个 size 16。
- 2 个 store-address issue queue，每个 size 16。
- 2 个 store-data issue queue，每个 size 16。
- `LQEntries=128`，`SQEntries=64`，这已经被前面的 `ldq/stq` fence.i 版本测到了清晰边界。

从这些配置看，理论候选区间应该类似：

| 项目 | 源码候选 |
| --- | ---: |
| `addsched` | 约 96 entries |
| `mulsched` | 约 32 entries |
| `loadsched` | 约 48 entries |
| `storeaddrsched` | 约 32 entries |
| `storedatasched` | 约 32 entries |

但这轮曲线没有在这些位置出现硬台阶，所以不能说“测量否定了源码配置”。更准确的说法是：这个测试方法暂时没有把这些结构的边界激发出来。

## 下一步建议

要继续测 scheduler 容量，我建议先改测试，再扫更多点：

1. 让 RISC-V structure helper 真的展开 `fillerInstrs1` 和 `fillerInstrs2` 两段，和 x86 版本更同构。
2. 给容量 runner 增加 single-point 模式：每次只生成/运行一个 x，并把对应 stats 保存下来。
3. 对候选点做小范围细扫，例如 `addsched=80:112:1`、`mulsched=24:40:1`、`loadsched=40:56:1`、`store*=24:40:1`。
4. 在 stats 里同时看 `system.cpu.scheduler.*.avgInsts`、`arbFailed`、issue/insert 分布，以及 rename/dispatch stall 原因；如果 cycles 没台阶但某个队列已满，就说明瓶颈被其它前后端行为掩盖了。

在这些改造前，当前最可信的 GEM5 AsmGen 结论仍然是基本吞吐/延迟和 `ldq/stq` 容量。
