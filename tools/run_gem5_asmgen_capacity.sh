#!/usr/bin/env bash
set -uo pipefail

GEM5_ROOT=${GEM5_ROOT:-/nfs/home/yanyue/workspace/GEM5_4}
GEM5_BIN=${GEM5_BIN:-$GEM5_ROOT/build/RISCV/gem5.opt}
GEM5_CONFIG=${GEM5_CONFIG:-$GEM5_ROOT/configs/example/se.py}
RISCV_PREFIX=${RISCV_PREFIX:-/nfs/home/share/riscv-toolchain-20230425-gcc12/bin/riscv64-unknown-linux-gnu-}
ITER=${ITER:-10}
TIMEOUT=${TIMEOUT:-300s}
MAXINSTS=${MAXINSTS:-0}
OUT_ROOT=${OUT_ROOT:-results/gem5_se_ideal_capacity_pf_$(date +%Y%m%d_%H%M%S)}

declare -A RANGES=(
    [rob]=240:400:4
    [ldq]=96:160:2
    [stq]=48:88:2
    [addsched]=64:128:2
    [mulsched]=20:56:2
    [loadsched]=32:72:2
    [storeaddrsched]=20:56:2
    [storedatasched]=20:56:2
    [mixloadstoresched]=32:96:2
    [intrf]=128:256:4
    [fprf]=160:320:4
    [faddsched]=40:112:2
    [fmulsched]=40:112:2
    [jmpsched]=32:96:2
    [ftq]=32:96:2
    [brq]=32:96:2
    [mdp]=32:160:4
    [returnstack]=1:96:1
)

if [ "$#" -gt 0 ]; then
    TESTS=("$@")
else
    TESTS=(rob ldq stq addsched mulsched loadsched storeaddrsched storedatasched)
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
    echo "maxinsts: $MAXINSTS"
    echo "tests: ${TESTS[*]}"
    echo "prefetch: default on; this script does not pass --no-pf"
    for test in "${TESTS[@]}"; do
        echo "range.$test: ${RANGES[$test]:-unset}"
    done
    if git -C "$GEM5_ROOT" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
        echo "gem5_branch: $(git -C "$GEM5_ROOT" rev-parse --abbrev-ref HEAD)"
        echo "gem5_commit: $(git -C "$GEM5_ROOT" rev-parse --short=12 HEAD)"
    fi
} > "$OUT_ROOT/manifest.txt"

printf "test\tstatus\trange\tpoints\tbaseline_first8\tfirst_ge_1p5x\tfirst_ge_2p0x\tmax_x\tmax_cycles\tm5out\tlog\n" > "$OUT_ROOT/summary.tsv"

summarize_points() {
    local test=$1
    local status=$2
    local range=$3
    local points_file=$4
    local m5out=$5
    local log_file=$6

    awk -v test="$test" -v status="$status" -v range="$range" \
        -v m5out="$m5out" -v log_file="$log_file" '
        BEGIN { FS = OFS = "\t" }
        {
            n++;
            xs[n] = $2;
            vs[n] = $3 + 0.0;
            if (n <= 8) {
                sum += vs[n];
                base_n++;
            }
        }
        END {
            if (n == 0) {
                print test, status, range, 0, "NA", "NA", "NA", "NA", "NA", m5out, log_file;
                exit;
            }

            baseline = sum / base_n;
            threshold15 = baseline * 1.5;
            threshold20 = baseline * 2.0;
            first15 = "NA";
            first20 = "NA";
            max_x = xs[1];
            max_v = vs[1];

            for (i = base_n + 1; i <= n; i++) {
                if (first15 == "NA" && vs[i] >= threshold15) {
                    first15 = xs[i];
                }
                if (first20 == "NA" && vs[i] >= threshold20) {
                    first20 = xs[i];
                }
            }

            for (i = 2; i <= n; i++) {
                if (vs[i] > max_v) {
                    max_v = vs[i];
                    max_x = xs[i];
                }
            }

            printf "%s\t%s\t%s\t%d\t%.6f\t%s\t%s\t%s\t%.6f\t%s\t%s\n", \
                test, status, range, n, baseline, first15, first20, max_x, max_v, m5out, log_file;
        }
    ' "$points_file"
}

for test in "${TESTS[@]}"; do
    range=${RANGES[$test]:-}
    if [ -z "$range" ]; then
        echo "No candidate range configured for $test" | tee "$OUT_ROOT/${test}.error.log"
        printf "%s\tmissing_range\tNA\t0\tNA\tNA\tNA\tNA\tNA\tNA\t%s\n" "$test" "$OUT_ROOT/${test}.error.log" >> "$OUT_ROOT/summary.tsv"
        continue
    fi

    echo "== build $test range $range =="
    if ! make -C AsmGen ARCH=riscv TIMING=cycle ONLY="$test" RANGE="$test=$range" CROSS_COMPILE_RISCV64="$RISCV_PREFIX" > "$OUT_ROOT/${test}.build.log" 2>&1; then
        printf "%s\tbuild_failed\t%s\t0\tNA\tNA\tNA\tNA\tNA\tNA\t%s\n" "$test" "$range" "$OUT_ROOT/${test}.build.log" >> "$OUT_ROOT/summary.tsv"
        continue
    fi

    m5out="/tmp/uarchtest-se-ideal-capacity-pf-${test}-${ITER}"
    log="$OUT_ROOT/${test}.gem5.log"
    points="$OUT_ROOT/${test}.tsv"

    echo "== run $test range $range =="
    timeout "$TIMEOUT" "$GEM5_BIN" \
        -d "$m5out" \
        "$GEM5_CONFIG" \
        --ideal-kmhv3 \
        --maxinsts "$MAXINSTS" \
        --cmd="$PWD/AsmGen/generate/clammicrobench_riscv" \
        --options="$test $ITER" > "$log" 2>&1
    status=$?

    awk -F, -v test="$test" '/^[0-9]+,[[:space:]]*[0-9.eE+-]+/ {
        gsub(/[[:space:]]/, "", $1);
        gsub(/[[:space:]]/, "", $2);
        print test "\t" $1 "\t" $2;
    }' "$log" > "$points"

    summarize_points "$test" "$status" "$range" "$points" "$m5out" "$log" >> "$OUT_ROOT/summary.tsv"
done
