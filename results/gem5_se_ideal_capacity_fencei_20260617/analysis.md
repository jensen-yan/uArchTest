# AsmGen RISC-V fence.i capacity probe

测试配置：

- gem5: `/nfs/home/yanyue/workspace/GEM5_4/build/RISCV/gem5.opt`
- 配置入口：`configs/example/se.py --ideal-kmhv3`
- AsmGen: RISC-V `TIMING=cycle`
- 修改点：RISC-V structure tests 在普通 `fence iorw, iorw` 后增加 `fence.i`
- 目的：给 `ldq/stq` 这类结构测试提供接近 x86 `lfence` 的 loop boundary，避免多个 iteration 同时堆进后端 LSQ。

## 背景

之前普通 RISC-V `fence iorw, iorw` 版本在 gem5 上会出现很早的 LSQ full：

- big-IQ 对照里 `ldq16` 的 `lqAvgEntryNum` 已经到 `117.56`
- `stq8` 的 `sqAvgEntryNum` 已经到 `55.95`

这不能解释成真实 LDQ/STQ 容量很小，而是说明普通 RISC-V `fence` 在 gem5 里不是 dispatch-serializing boundary。源码里普通 `fence` 是 `IsReadBarrier, IsWriteBarrier`，而 `fence_i` 带 `IsNonSpeculative, IsSerializeAfter`。

## 粗扫结果

`ldq`：

| x | cycles/iter |
| ---: | ---: |
| 16 | 35.92 |
| 64 | 59.28 |
| 96 | 72.98 |
| 120 | 77.04 |
| 128 | 79.70 |
| 136 | 100.46 |
| 160 | 88.76 |

`stq`：

| x | cycles/iter |
| ---: | ---: |
| 8 | 35.02 |
| 32 | 54.12 |
| 48 | 63.88 |
| 56 | 70.08 |
| 64 | 70.04 |
| 72 | 74.12 |
| 88 | 82.08 |

## Stats 验证

单点 stats 取第一个完整 stats block，用于确认台阶来源。

| test | x | cycles/iter | IQFull | LSQFull | DispBWFull | lqAvg | sqAvg | lqFullCycles | sqFullCycles |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| ldq | 120 | 69.44 | 16653 | 0 | 156400 | 72.01 | 0.28 | 0 | 0 |
| ldq | 128 | 71.12 | 17252 | 0 | 102462 | 72.83 | 0.28 | 89719 | 0 |
| ldq | 136 | 73.90 | 16644 | 89068 | 119775 | 55.06 | 0.21 | 89894 | 0 |
| ldq | 144 | 76.90 | 16572 | 78289 | 120523 | 51.28 | 0.22 | 79071 | 0 |
| stq | 48 | 55.24 | 19096 | 0 | 352804 | 1.70 | 33.81 | 0 | 0 |
| stq | 56 | 59.22 | 19765 | 0 | 312275 | 1.70 | 39.36 | 0 | 0 |
| stq | 64 | 62.82 | 23081 | 0 | 227376 | 1.69 | 43.94 | 0 | 191173 |
| stq | 72 | 66.86 | 23213 | 157021 | 216172 | 1.11 | 29.63 | 0 | 165449 |

## 结论

`fence.i` 版本恢复了更合理的容量形状：

- LDQ 的有效台阶出现在 `x ~= 128` 附近，和 `LQEntries=128` 对齐。
- STQ 的有效台阶出现在 `x ~= 64` 附近，和 `SQEntries=64` 对齐。
- 之前普通 `fence` 版本看到的“低 x 已经 full”主要是 loop iteration 重叠造成的，不表示实际 LDQ/STQ 只有很小容量。

注意：这里使用 `fence.i` 是 gem5 microbench 边界技巧，不是说真实 RISC-V 上 `fence.i` 等价 x86 `lfence`。真实语义上，`fence.i` 面向 instruction fetch/self-modifying code；本测试利用的是 gem5 当前对 `fence_i` 的 `SerializeAfter` 建模。
