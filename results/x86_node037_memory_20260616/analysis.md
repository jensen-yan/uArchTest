# x86 node037 Memory/Core 测试解读

测试机器：

- Host: `node037.bosccluster.com`
- CPU: AMD EPYC 9684X 96-Core Processor, 2 sockets, SMT2
- 运行绑定：单核测试用 `numactl --cpunodebind=0 --membind=0 taskset -c 0`
- socket0 多线程测试用 `numactl --cpunodebind=0 --membind=0 taskset -c 0-95`
- 双路测试用 `numactl --interleave=all taskset -c 0-191`

原始日志：

- `memlat_asm_iter5m.log`: assembly pointer-chasing latency，最大 1GB
- `memlat_c_iter5m.log`: C pointer-chasing latency，对照 C 循环开销
- `memlat_tlb_iter5m.log`: TLB 额外延迟测试
- `membw_single_size.tsv`: 单线程 data read/write/copy 带宽
- `membw_avx512_1gb_thread_scaling.tsv`: socket0，1GB total private read scaling
- `membw_avx512_3gb_thread_scaling.tsv`: socket0，3GB total private read scaling
- `membw_avx512_6gb_192t_2socket.tsv`: 双路 6GB total private read bandwidth
- `instr_bw_single_size.tsv`: instruction fetch bandwidth
- `boost_clock_core0_samples20.log`: core0 boost clock samples

## 怎么读

`MemoryLatency` 的第二列是每次 dependent pointer-chasing load 的平均延迟，单位 ns。它不是带宽，而是后一条 load 依赖前一条 load 地址时的串行延迟。

`MemoryBandwidth` 的第二列是 benchmark 自己统计的 GB/s。`-private` 下 size 是所有线程的总数据量，每个线程拿其中一份。因此：

- `1GB total private / 96 threads` 约等于每线程 10.7MB，总工作集仍小于单 socket 1152MB L3，主要看 3D V-Cache/L3 聚合带宽。
- `3GB total private / 96 threads` 明确超过单 socket L3，更接近本地 DDR5 实测读带宽。

`copy` 的 GB/s 按该 benchmark 的一次数组遍历计数，不一定等于实际读+写总线上字节数，所以更适合横向比较同一工具下的 copy 路径，不要直接当内存总线带宽。

## Core Clock

`BoostClockChecker` 绑定 core0，20 个样本：

| 指标 | GHz |
| --- | ---: |
| 全部样本中位数 | 3.679 |
| 跳过第一个 warmup 样本后的中位数 | 3.680 |
| 最大值 | 3.689 |

这个和 AMD 官方的 EPYC 9684X max boost 3.7GHz 对得上。下面把 ns 粗略换成 cycles 时，用 `3.68 cycles/ns` 估算。

## Memory Latency

| Region | asm ns/load | 约 cycles | 解释 |
| ---: | ---: | ---: | --- |
| 2KB | 1.094 | 4.0 | L1D hit，和 AsmGen `loadlat ~4 cycles` 一致 |
| 32KB | 1.209 | 4.5 | 接近 L1D 容量边缘 |
| 48KB | 3.819 | 14.1 | 超过 32KB L1D，进入 L2 hit 区间 |
| 256KB | 3.832 | 14.1 | L2 hit |
| 512KB | 5.044 | 18.6 | L2 内部/冲突/TLB 压力开始上升 |
| 1MB | 8.951 | 32.9 | 接近/超过每核 1MB L2，进入下一级路径 |
| 8MB | 17.739 | 65.3 | L3/片上路径 |
| 32MB | 31.026 | 114.2 | 仍可能在本 CCD 大 L3 内，但 TLB/random 代价明显 |
| 64MB | 52.111 | 191.8 | 接近 Genoa-X 单 CCD 96MB L3 的高压力区 |
| 128MB | 94.295 | 346.9 | 超过单 CCD L3 容量，片上/内存路径明显 |
| 1GB | 130.115 | 478.8 | 大工作集 random latency，本地 DRAM/片上路径综合 |

重点：

1. L1D latency 约 `4 cycles`，和 AsmGen 复测一致。
2. L2 latency 约 `14 cycles`，也符合公开 Zen 4 常见数据。
3. 1GB random pointer chasing 是 `~130ns`，比桌面 Zen 4 公开测试更高不奇怪：这是双路 EPYC/Genoa-X、DDR5 服务器平台、4KB page random pointer chasing 的实测环境。

## TLB

`MemoryLatency -test tlb` 输出的是“每页一个元素”相对 4KB 热数据参考的额外延迟，不是总 load latency。

| Page entries | 额外 ns | 约 cycles | 解读 |
| ---: | ---: | ---: | --- |
| 64 | 0.005 | 0.0 | 基本无额外代价 |
| 96 | 1.925 | 7.1 | 开始出现一级 DTLB 容量外代价 |
| 512 | 1.938 | 7.1 | 仍在较低额外代价平台 |
| 640 | 4.976 | 18.3 | 第二个明显台阶 |
| 2048 | 4.971 | 18.3 | 维持平台 |
| 3072 | 6.219 | 22.9 | 继续上升 |
| 4096 | 8.207 | 30.2 | 页数更多后 page-walk/cache 压力变大 |
| 8192 | 12.686 | 46.7 | 32MB footprint，每页访问一个元素，翻译代价明显 |

这里比较像多级 TLB/页表缓存逐步失效的曲线，不应该强行压成单个 TLB entry 数。

## 单线程带宽

| Method | 2KB | 32KB | 1MB | 32MB | 256MB | 1GB |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| AVX512 read | 216.1 | 208.0 | 97.2 | 86.1 | 33.3 | 33.0 |
| write | 114.4 | 114.5 | 89.2 | 91.9 | 34.8 | 26.0 |
| copy | 225.3 | 226.4 | 211.3 | 178.2 | 99.5 | 44.7 |

读法：

- 小工作集 read `~216 GB/s`，按 `3.68GHz` 约等于 `58.7 B/cycle`，接近每周期两条 256-bit load 的 Zen 4 L1D 路径。
- 1GB 单线程 read `~33 GB/s`，这是一个核心跑大流式读的能力，不是 socket 总带宽。
- write/copy 的数值受 benchmark 计数方式、write allocate、store buffer、非临时写策略影响，先作为工具内横向对照。

## 多线程读带宽

socket0，`1GB total private`：

| Threads | GB/s |
| ---: | ---: |
| 1 | 34.3 |
| 2 | 64.3 |
| 4 | 121.2 |
| 8 | 446.3 |
| 16 | 638.3 |
| 32 | 1373.4 |
| 48 | 1402.0 |
| 64 | 1544.7 |
| 96 | 2787.8 |

这组不是 DRAM 带宽。1GB 总工作集小于单 socket 1152MB L3，96 线程时主要测到的是 Genoa-X 大 L3/V-Cache 聚合读带宽。

socket0，`3GB total private`：

| Threads | GB/s |
| ---: | ---: |
| 1 | 34.0 |
| 2 | 62.0 |
| 4 | 119.0 |
| 8 | 186.4 |
| 16 | 260.4 |
| 32 | 264.1 |
| 48 | 287.0 |
| 64 | 333.0 |
| 96 | 365.2 |

这组更接近单 socket 本地 DRAM 读带宽。96 物理核读到 `~365 GB/s`，低于 AMD 官方每 socket DDR5 理论 `460.8 GB/s`，属于实际 AVX512 read loop、NUMA、本地内存、频率和内存控制器共同作用下的实测值。

双路，`6GB total private`，192 物理核，interleave all：

| Threads | GB/s |
| ---: | ---: |
| 192 | 624.8 |

双路总读带宽大约是单路 3GB/96 线程的 `1.71x`，没有线性到 `2x`。这可能和 interleave 粒度、线程调度、socket 间均衡、AVX512 频率、电源限制有关。

## Instruction Fetch Bandwidth

| Method | 2KB | 32KB | 256KB | 1MB | 8MB |
| --- | ---: | ---: | ---: | ---: | ---: |
| `instr8` | 165.2 | 110.8 | 58.0 | 53.8 | 43.0 |
| `instr4` | 164.8 | 95.7 | 58.1 | 53.3 | 44.2 |

这个测试是生成 NOP 代码块并执行，单位是 GB/s。2KB 热代码时 instruction-side 带宽很高；代码 footprint 增大后降到 `~50GB/s` 量级。它和 AsmGen `nopbw ~12 NOP/cycle` 不是同一个单位：AsmGen 看 NOP 指令条数/周期，这里看 instruction bytes/s。

## 已完成和建议下一步

这轮已经完成：

- AsmGen 全量 + 可疑项复测
- MemoryLatency asm/c/tlb
- MemoryBandwidth 单线程 read/write/copy
- MemoryBandwidth socket0 1GB/3GB 多线程 read scaling
- MemoryBandwidth 双路 6GB read bandwidth
- instruction fetch bandwidth
- core0 boost clock

还没跑、但值得作为下一阶段：

1. `CoherencyLatency`: 选少量 core pair，测同 CCD、跨 CCD、跨 socket cache-line handoff latency。
2. `MemoryLatency -test stlf/matched_stlf/dword_stlf`: store-to-load forwarding 64x64 矩阵，适合单独成图。
3. 修 `MemoryLatency -test mlp` 的矩阵输出循环 bug 后再跑 MLP。
4. `InstructionRate`: 只挑 AVX512/FMA/AES/PDEP/PEXT 等重点项跑，不建议直接全量跑。
