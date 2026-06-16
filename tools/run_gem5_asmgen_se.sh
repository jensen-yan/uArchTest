#!/usr/bin/env bash
set -uo pipefail

GEM5_ROOT=${GEM5_ROOT:-/nfs/home/yanyue/workspace/GEM5_4}
GEM5_BIN=${GEM5_BIN:-$GEM5_ROOT/build/RISCV/gem5.opt}
GEM5_CONFIG=${GEM5_CONFIG:-$GEM5_ROOT/configs/example/se.py}
RISCV_PREFIX=${RISCV_PREFIX:-/nfs/home/share/riscv-toolchain-20230425-gcc12/bin/riscv64-unknown-linux-gnu-}
ITER=${ITER:-100}
TIMEOUT=${TIMEOUT:-180s}
OUT_ROOT=${OUT_ROOT:-results/gem5_se_ideal_pf_$(date +%Y%m%d_%H%M%S)}

if [ "$#" -gt 0 ]; then
    TESTS=("$@")
else
    TESTS=(nopbw addbw mulbw loadbw addlat mullat loadlat)
fi

mkdir -p "$OUT_ROOT"

{
    echo "date: $(date -Iseconds)"
    echo "host: $(hostname)"
    echo "gem5_root: $GEM5_ROOT"
    echo "gem5_bin: $GEM5_BIN"
    echo "gem5_config: $GEM5_CONFIG"
    echo "riscv_prefix: $RISCV_PREFIX"
    echo "iter: $ITER"
    echo "timeout: $TIMEOUT"
    echo "tests: ${TESTS[*]}"
    if git -C "$GEM5_ROOT" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
        echo "gem5_branch: $(git -C "$GEM5_ROOT" rev-parse --abbrev-ref HEAD)"
        echo "gem5_commit: $(git -C "$GEM5_ROOT" rev-parse --short=12 HEAD)"
    fi
    echo "prefetch: default on; this script does not pass --no-pf"
} > "$OUT_ROOT/manifest.txt"

printf "test\tstatus\treported_x\treported_cycles_per_op\top_per_cycle\tstats_ipc\tstats_cpi\tstats_simInsts\tstats_numCycles\tstats_simTicks\thostSeconds\tm5out\tlog\n" > "$OUT_ROOT/summary.tsv"

extract_stat() {
    local file=$1
    local key=$2
    awk -v key="$key" '$1 == key { value = $2 } END { if (value != "") print value; else print "NA" }' "$file"
}

for test in "${TESTS[@]}"; do
    echo "== build $test =="
    if ! make -C AsmGen ARCH=riscv TIMING=cycle ONLY="$test" CROSS_COMPILE_RISCV64="$RISCV_PREFIX" > "$OUT_ROOT/${test}.build.log" 2>&1; then
        printf "%s\tbuild_failed\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\t%s\n" "$test" "$OUT_ROOT/${test}.build.log" >> "$OUT_ROOT/summary.tsv"
        continue
    fi

    m5out="/tmp/uarchtest-se-ideal-pf-${test}-${ITER}"
    log="$OUT_ROOT/${test}.gem5.log"
    echo "== run $test =="
    timeout "$TIMEOUT" "$GEM5_BIN" \
        -d "$m5out" \
        "$GEM5_CONFIG" \
        --ideal-kmhv3 \
        --cmd="$PWD/AsmGen/generate/clammicrobench_riscv" \
        --options="$test $ITER" > "$log" 2>&1
    status=$?

    reported=$(awk -F, '/^[0-9]+,[[:space:]]*[0-9.eE+-]+/ {
        gsub(/[[:space:]]/, "", $1);
        gsub(/[[:space:]]/, "", $2);
        print $1 "\t" $2;
        exit;
    }' "$log")
    reported_x=$(printf "%s" "$reported" | awk '{print $1}')
    reported_val=$(printf "%s" "$reported" | awk '{print $2}')
    if [ -z "$reported_x" ]; then reported_x="NA"; fi
    if [ -z "$reported_val" ]; then reported_val="NA"; fi

    if [ "$reported_val" != "NA" ]; then
        op_per_cycle=$(awk -v v="$reported_val" 'BEGIN { if (v > 0) printf "%.6f", 1.0 / v; else print "NA" }')
    else
        op_per_cycle="NA"
    fi

    stats="$m5out/stats.txt"
    if [ -f "$stats" ]; then
        sim_ticks=$(extract_stat "$stats" "simTicks")
        host_seconds=$(extract_stat "$stats" "hostSeconds")
        sim_insts=$(extract_stat "$stats" "simInsts")
        num_cycles=$(extract_stat "$stats" "system.cpu.numCycles")
        cpi=$(extract_stat "$stats" "system.cpu.cpi")
        ipc=$(extract_stat "$stats" "system.cpu.ipc")
    else
        sim_ticks="NA"
        host_seconds="NA"
        sim_insts="NA"
        num_cycles="NA"
        cpi="NA"
        ipc="NA"
    fi

    printf "%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n" \
        "$test" "$status" "$reported_x" "$reported_val" "$op_per_cycle" \
        "$ipc" "$cpi" "$sim_insts" "$num_cycles" "$sim_ticks" \
        "$host_seconds" "$m5out" "$log" >> "$OUT_ROOT/summary.tsv"
done
