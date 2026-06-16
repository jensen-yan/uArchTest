# AsmGen x86 node037 数据解读

测试机器：

- Host: `node037.bosccluster.com`
- CPU: AMD EPYC 9684X 96-Core Processor, 2 sockets, SMT2
- 架构判断：EPYC 9684X 属于 EPYC 9004 / Genoa-X，核心微架构是 Zen 4，不是 Zen 4c
- 运行绑定：`taskset -c 0`
- 主测试命令：`./AsmGen/generate/clammicrobench_x86 <test> 100`
- 补充吞吐/延迟校准：`perf stat -e cycles,instructions -- ./AsmGen/generate/clammicrobench_x86 <test> 100000`

原始日志：

- `asmgen_all_iter100.log`: AsmGen 全测试一轮，除 `indirectbranch` 外都在 60s 内完成
- `asmgen_indirectbranch_iter100.log`: `indirectbranch` 单独补跑完整结果
- `quick_summary_warm.tsv`: 自动提取的粗略拐点摘要
- `retest/*_iter1000_repeats5.log`: 对可疑项的 `iter=1000`、重复 5 次复测
- `retest_summary.tsv`: 复测后按每个 x 取 5 次中位数得到的摘要
- `retest_focus.tsv`: 复测关键区间的中位数/min/max 小表
- `retest_rob_btb/*`: 对 `ROB` 和 `BTB` 的追加复测；包含重复日志、中位数表、关键区间表

公开资料：

- AMD 官方规格页：[AMD EPYC 9684X](https://www.amd.com/en/products/processors/server/epyc/4th-generation-9004-and-8004-series/amd-epyc-9684x.html)
- Chips and Cheese Zen 4 逆向分析：[Part 1: Frontend and Execution Engine](https://chipsandcheese.com/p/amds-zen-4-part-1-frontend-and-execution-engine)
- Chips and Cheese Zen 4 逆向分析：[Part 2: Memory Subsystem and Conclusion](https://chipsandcheese.com/p/amds-zen-4-part-2-memory-subsystem-and-conclusion)

AMD 官方规格显示 EPYC 9684X 是 96C/192T、最高 boost 3.7 GHz、all-core boost 3.42 GHz、base 2.55 GHz、L3 1152 MB、SP5、1P/2P、DDR5 12 通道、支持 AMD 3D V-Cache。当前机器 `lscpu` 显示 384 logical CPUs、2 socket、每 socket 96 cores、SMT2、总 L3 约 2.3 GiB，和 2P x 1152 MB 完全对上。

## 数值怎么读

AsmGen 里有两类测试，读法不一样。

### 吞吐/延迟类

例如：

```text
nopbw:
512,0.026797
```

这里的 `512` 是测试函数名里的规模，也就是循环体里放了 512 条被测指令。第二列不是整个 512 条指令的总时间，而是：

```text
总时间 / (iterations * 512)
```

也就是每条被测指令的平均时间。`nopbw/addbw/mulbw/loadbw/addlat/mullat/loadlat/btb/branchhist/indirectbranch/returnstack/mdp` 这类 `DivideTimeByCount=true` 的测试基本按这个思路读。

不过，全量日志里这些超短吞吐测试只用 `iter=100`，时间太短，`clock_gettime` 和启动噪声会让 ns 数字偏乐观。因此吞吐/延迟结论主要看后面 `perf + iter=100000` 的补测。

### 容量/压力类

例如 `rob/ldq/stq/intrf/addsched/loadsched` 这类 `DivideTimeByCount=false` 的测试，第二列是每次结构测试循环的平均时间。它们不是直接给“每条指令多少 ns”，而是看 `x` 逐步增大时什么时候明显变慢。

所以 `ldq` 摘要里写的 `137:70.3` 不是精确地说 LDQ 有 137 项，而是说：

```text
在这个测试方法、这个阈值规则下，x=137 开始首次超过 warm baseline 的 1.5 倍
```

它只能当“拐点候选”。如果要把它解释成硬件 LDQ entry 数，需要围绕 `120..160` 做更高迭代、多次重复、取中位数/分位数，再结合 uarch 已知规格校对。

复测后，`ldq` 的 `x=137` 台阶是稳定复现的：`x=136` 的 5 次中位数约 `64.910 ns`，`x=137` 约 `89.011 ns`。这仍然更适合叫“LDQ 压力拐点候选”，不要写成“LDQ 精确等于 137 项”。

## 吞吐和延迟换算

下面是用 `iter=100000` 加 `perf cycles/instructions` 补测得到的更可信结果。

| 测试 | 测什么 | ns/op | cycles/op | op/cycle | 解释 |
| --- | --- | ---: | ---: | ---: | --- |
| `nopbw512` | NOP 前端/retire 吞吐 | 0.102380 | 0.085 | 11.7 | 可以近似说一拍 12 条 NOP |
| `addbw512` | 独立整数 add 吞吐 | 0.243035 | 0.254 | 3.94 | 约一拍 4 条整数 add |
| `mulbw512` | 独立 64-bit `imul` 吞吐 | 0.322600 | 1.01 | 0.99 | 约一拍 1 条整数乘法 |
| `loadbw512` | 独立 64-bit L1 load 吞吐 | 0.276296 | 0.339 | 2.95 | 约一拍 3 条 load |
| `addlat512` | 依赖 add 链延迟 | 0.965014 | 1.01 | - | add latency 约 1 拍 |
| `mullat512` | 依赖 `imul` 链延迟 | 1.942156 | 3.03 | - | imul latency 约 3 拍 |
| `loadlat512` | 依赖 load-use 链延迟 | 1.932711 | 4.04 | - | L1 dependent load 约 4 拍 |

注意：`nopbw` 的 12 条/拍不能等价成“普通有用指令也能 12 条/拍”。NOP 在 x86 上常有特殊处理，它更像前端/NOP 消除/retire 路径的上限观测。`addbw` 这种 4 条/拍更接近普通整数执行吞吐。

## 主要容量/压力测试怎么理解

### 已复测的可疑项

下面几项已经用 `iter=1000`、重复 5 次复测，并对每个 `x` 取中位数。

| 测试 | 复测结论 | 关键中位数 |
| --- | --- | --- |
| `ldq` | 稳定台阶在 `x=137` | `x=136: 64.910 ns`, `x=137: 89.011 ns`, 后续维持约 90-100 ns |
| `stq` | 第一轮 `x=28` 是误判；稳定台阶更像 `x=67` | `x=66: 54.131 ns`, `x=67: 84.301 ns`, 后续约 85-95 ns |
| `loadsched` | 第一轮 `x=30` 是误判；稳定台阶更像 `x=48` | `x=47: 62.381 ns`, `x=48: 89.421 ns`, 后续约 90-95 ns |
| `simd256rf` | 第一轮 `x=133` 的巨大尖峰没有复现；不是硬拐点 | `x=133: 37.110 ns`, `x=150: 41.960 ns`, `x=173: 48.521 ns`; 整体是 150 后逐步爬升 |

复测摘要：

| 测试 | 5 次中位数 warm baseline | 1.5x 阈值 | 首个超过阈值的 x | 前 16 点中位数 | 后 16 点中位数 |
| --- | ---: | ---: | ---: | ---: | ---: |
| `ldq` | 48.621 ns | 72.931 ns | 137 | 63.785 ns | 95.031 ns |
| `stq` | 47.650 ns | 71.475 ns | 67 | 53.941 ns | 89.906 ns |
| `loadsched` | 55.110 ns | 82.665 ns | 48 | 60.236 ns | 91.486 ns |
| `simd256rf` | 32.341 ns | 48.511 ns | 173 | 46.380 ns | 50.126 ns |

`simd256rf` 这个自动阈值的 `x=173` 只表示慢慢爬升后首次过阈值，不像 `ldq/stq/loadsched` 那样有明显台阶。

### ROB / BTB 追加复测

用户指出 `ROB` 和 `BTB` 与公开 Zen 4 数据还不够一致，所以这两项又单独复测了一轮。

`ROB` 默认测试用 `iter=300`、重复 5 次，对每个 `x` 取中位数。结果不是 `x=320` 附近的硬台阶，而是随 NOP 填充长度逐步上升：

| x 区间 | 中位数 ns/iteration |
| --- | ---: |
| 65-128 | 36.132 |
| 129-192 | 37.415 |
| 193-256 | 38.652 |
| 257-320 | 40.252 |
| 321-384 | 41.148 |
| 449-512 | 44.467 |
| 641-768 | 49.150 |
| 897-1024 | 55.800 |

又临时把 `ROB` 扫描范围扩到 `4..2048 step=4`，同样 `iter=300`、重复 5 次。扩展扫描仍然是平滑上升：

| x 区间 | 中位数 ns/iteration |
| --- | ---: |
| 4-256 | 36.252 |
| 260-512 | 41.333 |
| 516-768 | 47.052 |
| 772-1024 | 53.350 |
| 1028-1280 | 59.567 |
| 1284-1536 | 68.833 |
| 1540-1792 | 74.533 |
| 1796-2048 | 81.185 |

这个结果说明：当前 AsmGen 的 `rob` 测试不适合直接验证 “Zen 4 ROB = 320 entries”。它的循环形状是两条 pointer-chasing load，中间塞 NOP，最后 `lfence`，所以测到的是“在这种 NOP 填充下，前后长延迟 load 的 overlap 能保持到多长”的曲线。Zen 4 对 NOP 有特殊打包/融合处理，公开逆向也提到纯 NOP 可以测到远大于 320 的窗口；因此这里没有在 320 断开并不矛盾。要确认 320，需要另写一个混合 uop ROB 测试，不能继续用全 NOP 填充的 `rob` 当容量计数器。

`BTB` 默认五个 spacing 用 `iter=100`、重复 5 次复测，下面是每个 branch target 的中位数 ns：

| 测试 | 512 | 1024 | 1536 | 2048 | 3072 | 4096 | 8192 | 10240 | 16384 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| `btb4Unconditional` | 0.494 | 0.497 | 0.352 | 0.293 | 0.308 | 0.380 | 0.498 | 1.304 | 3.299 |
| `btb8Unconditional` | 0.278 | 0.421 | 0.277 | 0.279 | 0.285 | 0.330 | 0.460 | 1.668 | 3.259 |
| `btb16Unconditional` | 0.277 | 0.277 | 0.277 | 0.281 | 0.414 | 0.784 | 0.594 | 2.569 | 3.315 |
| `btb32Unconditional` | 0.278 | 0.277 | 0.917 | 0.433 | 0.428 | 0.769 | 3.284 | 3.343 | 3.298 |
| `btb64Unconditional` | 1.039 | 0.488 | 0.521 | 0.663 | 0.542 | 0.829 | 0.988 | 2.612 | 3.414 |

复测后更合理的读法是：AsmGen 的 BTB 测试不是单一容量测试，而是 “容量 + branch density + BTB set/index 冲突 + i-cache footprint” 混合测试。`btb16` 最好读：到 2048 基本平，3072 开始抬，4096 明显抬，8192 还能撑住，10240 大跳；这和公开的 Zen 4 L1 BTB 受密度影响约 1024-3072、L2 BTB 约 8192 的分层比较一致。`btb32/btb64` 的早期尖峰更像特定 spacing 的 index 冲突，不应读成 “BTB 只有 512/1536 项”。

## 和公开 Zen 4 数据对照

这台机子是 EPYC 9684X，也就是 Genoa-X / Zen 4 + 3D V-Cache。Genoa-X 的大 L3 主要影响缓存容量和部分内存工作负载；单核前端、rename、scheduler、load/store queue 这些核心内结构基本按 Zen 4 来理解。

| 项目 | 我们测到 | 公开资料 | 对照结论 |
| --- | ---: | ---: | --- |
| CPU/平台 | 2P, 96C/socket, SMT2, 总 L3 约 2.3 GiB | AMD 官方：9684X 是 96C/192T、1152 MB L3、1P/2P、3D V-Cache | 完全吻合；这就是双路 Genoa-X |
| NOP 吞吐 | `11.7 NOP/cycle` | Chips and Cheese：Zen 4 从 uop cache 可测到约 12 NOP/cycle；他们认为 NOP 被特殊成对/打包处理 | 强吻合；所以 `nopbw` 的 12 条/拍可信，但只代表 NOP 特例 |
| 普通整数 add 吞吐 | `3.94 add/cycle` | Zen 4 后端执行资源和 Zen 3 类似，整数侧可提供多 ALU 并行执行 | 趋势合理；约 4 条/拍符合 Zen 系列整数 ALU 直觉 |
| 64-bit `imul` 吞吐/延迟 | `~1/cycle`, latency `~3 cycles` | 公开资料通常认为 Zen 4 整数乘法吞吐约一拍一条、延迟约三拍 | 吻合 |
| dependent L1 load latency | `4.04 cycles` | Chips and Cheese：Zen 4 L1D hit latency 是 4 cycles | 强吻合 |
| scalar load 吞吐 | `2.95 load/cycle` | Chips and Cheese：Zen 4 有 3 AGU；L1D 宽度可支撑高 load 带宽 | 吻合；我们这里主要测到 load uop/AGU 侧约 3/cycle |
| LDQ / load in-flight 压力 | `x=137` 处稳定台阶 | Chips and Cheese：Zen 4 可保持约 136 个 load operations in flight；他们区分 load execution queue 和 load validation queue | 非常强吻合；这解释了为什么不是公开 “load queue 88” 那个数字 |
| Store Queue 压力 | `x≈67` | Chips and Cheese：Zen 4 store queue 是 64 entries | 强吻合；多出来几项可能来自测试计数方式/前后指令/阈值 |
| Return stack | 第一轮 `returnstack` 在 33 后明显变慢 | Chips and Cheese：Zen 4 return stack 是 32 entries，单线程可用完整 32 项 | 强吻合 |
| ROB | 复测没有 320 硬台阶；`4..2048` 扩展扫描也是平滑上升 | Chips and Cheese：Zen 4 ROB 是 320 entries；纯 NOP 会被特殊打包，甚至能测到远超 320 的 NOP 容量 | 当前 AsmGen `rob` 不能当 ROB entry 计数器；结果更像 NOP 填充窗口测试 |
| BTB | `btb16` 在 3072/4096 抬升、10240 大跳；其他 spacing 有局部早期冲突 | Chips and Cheese：Zen 4 L1 BTB 实测受 branch density 影响，可在 1024-3072 targets；L2 BTB 约 8192 | 复测后和公开分层更一致；spacing 冲突不能读成单一容量 |
| SIMD256 RF | 未见硬台阶，约 150 后逐步爬升 | Chips and Cheese：Zen 4 vector RF 扩展到 512-bit，并保留较强 256-bit/vector 执行资源 | 不矛盾；我们的 `simd256rf` 更像逐步资源压力，不是清晰容量阈值 |

最有价值的交叉验证是这三条：

1. `nopbw` 的 `~12 NOP/cycle` 和公开逆向结果几乎一致。
2. `loadlat` 的 `~4 cycles` 和公开 L1D latency 一致。
3. `ldq x=137` 与公开的 `136 loads in flight` 几乎正好对上。

这里也解释了前面那个问题：`ldq` 的 137 不是传统意义上 “LDQ entry = 137”。按 Chips and Cheese 的说法，AMD Zen 的 load side 至少有两个相关概念：一个是更小的 load execution queue，另一个是更大的 load validation / retirement tracking 结构。我们的 `ldq` 测试描述就是 “loads pending retire”，因此它测到的更像后者的 in-flight load 跟踪能力，所以和 136 对齐，而不是和公开常见的 load execution queue 数字对齐。

### 全量第一轮粗略表

| 测试 | 目标 | 这轮粗略拐点候选 | 怎么解释 |
| --- | --- | ---: | --- |
| `rob` | NOP 填充窗口压力，不是干净 ROB 计数 | 无硬台阶；到 1024 平滑升至约 55.8 ns | 复测确认不应写 `x≈200`；需要混合 uop 测试才能验证公开 320-entry ROB |
| `ldq` | Load Queue，loads pending retire | x 约 137 | 已复测确认稳定台阶；仍不要直接等同精确 LDQ entry 数 |
| `stq` | Store Queue，stores pending retire | x 约 67 | 第一轮 `x=28` 是噪声/规则误判，复测后修正为约 67 |
| `mixldqstq` | 混合 load/store queue 压力 | x 约 124 | 混合内存操作待退休压力拐点 |
| `ftq` | taken branch / fetch target queue 压力 | x 约 64 | taken branch pending retire 的前端目标队列压力 |
| `brq` | not-taken branch reorder queue 压力 | x 约 64 | not-taken branch pending retire 压力 |
| `intrf` | Integer physical register file 压力 | x 约 45 | 整数寄存器相关压力；这轮噪声/异常点较多，需复测 |
| `fprf` | FP physical register file 压力 | x 约 181 | FP/scalar RF 压力候选 |
| `flagrf` | flags/condition-code 相关资源压力 | x 约 238 | flags rename/保存资源的压力候选 |
| `simd128rf` | 128-bit SIMD RF 压力 | x 约 154 | 128-bit SIMD 寄存器资源候选 |
| `simd256rf` | 256-bit SIMD RF 压力 | 无明显硬台阶；约 150 后逐步爬升 | 第一轮 `x=133` 异常尖峰未复现，不应当成容量点 |
| `simd512rf` | 512-bit SIMD RF 压力 | x 约 137 | AVX-512 路径，频率/功耗状态可能影响明显 |
| `addsched` | integer add scheduler 容量压力 | x 约 91 | 整数 add scheduler/issue queue 压力候选 |
| `mulsched` | integer mul scheduler 容量压力 | x 约 44 | 乘法相关 scheduler 压力候选 |
| `faddsched` | FP add scheduler 容量压力 | x 约 115 | FP add scheduler 压力候选 |
| `fmulsched` | FP mul scheduler 容量压力 | x 约 132 | FP mul scheduler 压力候选 |
| `loadsched` | load scheduler 容量压力 | x 约 48 | 第一轮 `x=30` 是噪声/规则误判，复测后修正为约 48 |
| `storeaddrsched` | store address scheduler 压力 | x 约 65 | store-address 侧调度压力 |
| `storedatasched` | store data scheduler 压力 | x 约 64 | store-data 侧调度压力 |
| `mixloadstoresched` | load/store scheduler 混合压力 | x 约 97 | 混合 load/store 调度资源压力 |

这些 “x 约多少” 都来自 `quick_summary_warm.tsv` 的自动规则：

```text
warm baseline = 跳过前若干点后的早期中位数
拐点候选 = 第一个 >= 1.5 * warm baseline 的点
```

这个规则很粗，只适合帮人快速定位区域。正式结论建议对候选区域做细扫。

## BTB / 分支相关

BTB 测试的第二列是每个 branch target 的平均 ns。复测后不要再用“第一个超过 1.5x 的点”给 BTB 下单一容量结论。更可靠的现象是：

- `btb16Unconditional` 最接近公开分层：`2048` 前几乎平，`3072` 开始抬，`4096` 明显抬，`8192` 仍可运行在中等代价，`10240` 出现大跳。
- `btb4/btb8` 在小 count 处有一些非单调变化，说明非常密集的 branch 会改变 BTB packing/index 行为。
- `btb32/btb64` 有早期尖峰，尤其 `btb64` 在 `512` 就很慢，像 spacing 造成的 set/index 冲突，而不是容量真的只有 512。

所以 BTB 的结论应写成：Zen 4 的 BTB 行为和公开资料的 “L1 约 1024-3072、L2 约 8192” 大体一致，但 AsmGen 当前这个 spacing sweep 测的是布局敏感行为，不能压成一个数字。

`branchhist` 和 `indirectbranch` 是二维矩阵：

- `branchhist`: 行是 branch count，列是 history length；结果分 random 和 predictable 两张表。
- `indirectbranch`: 行是 indirect branch count，列是 target count；结果分 test 和 reference 两张表。

这两项更像预测器行为图，不适合压成一个数字。

## 推荐下一步复测

如果目标是把这些结果当成 x86 主机的稳定性能资料，下一步建议：

1. 吞吐/延迟：保留 `iter=100000`，重复 5 次，直接记录 cycles/op。
2. `rob`: 不再复测当前全 NOP 版本；要新增一个混合 uop ROB 测试，避免 NOP 打包影响。
3. `btb`: 增加 varied-spacing、conditional、ZenMix 三类变体，并固定函数起始对齐，区分容量和 set/index 冲突。
4. `mixldqstq`: 对 `x=100..150` 做重复复测，看混合 load/store 队列拐点是否稳定在 120 多。
5. `storeaddrsched/storedatasched`: 对 `x=48..80` 做重复复测，和 `stq` 的 `x≈67` 对齐看。
6. SIMD/AVX 项：复测时记录当前频率，AVX/AVX-512 负载可能带来频率状态变化。
