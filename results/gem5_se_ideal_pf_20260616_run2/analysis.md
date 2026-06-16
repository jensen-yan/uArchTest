# AsmGen gem5 SE ideal_kmhv3 数据解读

测试配置：

- Host: `node037.bosccluster.com`
- gem5: `/nfs/home/yanyue/workspace/GEM5_4/build/RISCV/gem5.opt`
- 配置入口：`configs/example/se.py --ideal-kmhv3`
- Prefetch: 默认打开，没有传 `--no-pf`
- AsmGen: RISC-V `TIMING=cycle`
- 主结果：`summary.tsv`
- `loadlat` 补跑：`../gem5_se_ideal_pf_20260616_loadlat_iter10/summary.tsv`

## 结果总览

| 测试 | gem5 cycles/op | gem5 op/cycle | x86 cycles/op | x86 op/cycle | 源码一阶预期 |
| --- | ---: | ---: | ---: | ---: | --- |
| `nopbw` | 0.167000 | 5.99 | 0.085 | 11.7 | 约 6-8 op/cycle |
| `addbw` | 0.167000 | 5.99 | 0.254 | 3.94 | 约 6 op/cycle |
| `mulbw` | 0.500005 | 2.00 | 1.01 | 0.99 | 约 2 op/cycle |
| `loadbw` | 0.333339 | 3.00 | 0.339 | 2.95 | 约 3 load/cycle |
| `addlat` | 1.000004 | 1.00 | 1.01 | - | 约 1 cycle |
| `mullat` | 3.000004 | 0.33 | 3.03 | - | 约 3 cycles |
| `loadlat` | 4.000068 | 0.25 | 4.04 | - | 约 4 cycles |

`loadlat` 的 `ITER=100` 超时，没有程序输出；表里采用 `ITER=10` 补跑结果。这个测试是依赖 load 链，缩小迭代数不会改变每条 load 的一阶 latency 读法。

## 怎么解释

这批结果基本贴着源码参数走：

- `addbw` 接近 6 add/cycle：`KMHV3Scheduler` 里有 6 个 `IntALU` capable issue port。
- `mulbw` 接近 2 mul/cycle：`KMHV3Scheduler` 里有 2 个 `IntMult` capable issue port。
- `loadbw` 接近 3 load/cycle：`KMHV3Scheduler` 里有 3 个 `ReadPort` load issue queue。
- `addlat/mullat/loadlat` 分别接近 1/3/4 cycles：对应 `IntALU` 默认 1 cycle、`IntMult` 3 cycles、`MemRead` 4 cycles。

和 x86 相比，最显著的是：

- x86 `nopbw` 更高，约 11.7 NOP/cycle；gem5 ideal 是约 6 NOP/cycle。NOP 不能直接当普通有用指令吞吐，尤其 x86 可能有特殊处理。
- gem5 ideal 的 `addbw/mulbw` 比本机 x86 更高，正好反映这个模型配置了 6 条 IntALU 和 2 条 IntMult issue 能力。
- `loadbw/addlat/mullat/loadlat` 和 x86 数值非常接近，但原因不同：这里更多是 gem5 ideal 参数直接设出来的结果，不代表已经校准到 EPYC 9684X。

## Prefetch 状态

这轮是默认打开 prefetch。`config.ini` 中能看到：

- `system.cpu.dcache.prefetcher`
- `system.l2_wrappers.prefetcher`
- 各个 `system.l2_wrappers.slices*.inner_cache.prefetcher`
- `system.l3.prefetcher`

之前默认 PF 路线会在配置阶段 assert，是因为 SE 默认把 aligned L2 的 `l2_hwp_type` 设成了 `WorkerPrefetcher`；aligned L2 的 inner-cache 侧需要 `PrefetcherForwarder`，真正的 L2 prefetcher 挂在 wrapper 上。当前本地已修成默认 PF 可跑。

## Caveats

- AsmGen throughput loop 的 denominator 只包含 512 条 payload 指令；循环尾 `addi a0,-1` 和 `bnez` 会被摊薄但不计入 denominator。
- RISC-V `loadbw` 是同一 cache line 内 6 个固定 offset load；x86 analysis 里的 x86 `loadbw` 是 8 个 offset。
- `mulbw/mullat` 用的是零初始化输入，理论上如果模型或硬件有数据相关快路径，可能不代表一般 operand。
- 这批数据适合做 gem5 模型 sanity check 和与 x86 的定性对比；不应读成真实 XiangShan tapeout 或 EPYC 的校准结论。
