#!/usr/bin/env bash
set -uo pipefail

GEM5_ROOT=${GEM5_ROOT:-/nfs/home/yanyue/workspace/GEM5_4}
GEM5_BIN=${GEM5_BIN:-$GEM5_ROOT/build/RISCV/gem5.opt}
GEM5_CONFIG=${GEM5_CONFIG:-$GEM5_ROOT/configs/example/se.py}
RISCV_PREFIX=${RISCV_PREFIX:-/nfs/home/share/riscv-toolchain-20230425-gcc12/bin/riscv64-unknown-linux-gnu-}
ITER=${ITER:-10}
TIMEOUT=${TIMEOUT:-180s}
MAXINSTS=${MAXINSTS:-0}
OUT_ROOT=${OUT_ROOT:-results/gem5_se_ideal_points_$(date +%Y%m%d_%H%M%S)}
GEM5_EXTRA_ARGS=${GEM5_EXTRA_ARGS:-}
GEM5_EXTRA_ARGS_ARY=()
if [ -n "$GEM5_EXTRA_ARGS" ]; then
    # Keep this intentionally simple: pass space-separated gem5 config args,
    # e.g. GEM5_EXTRA_ARGS='--param=system.cpu.scheduler.IQs[0:18].size=256'
    read -r -a GEM5_EXTRA_ARGS_ARY <<< "$GEM5_EXTRA_ARGS"
fi

declare -A RANGES=(
    [rob]=120:200:4
    [ldq]=112:144:2
    [stq]=48:80:2
    [intrf]=160:256:4
    [fprf]=192:320:4
    [addsched]=80:112:2
    [mulsched]=24:44:2
    [faddsched]=56:88:2
    [fmulsched]=56:88:2
    [loadsched]=40:56:2
    [storeaddrsched]=24:44:2
    [storedatasched]=24:44:2
    [mixloadstoresched]=40:72:2
)

declare -A TARGET_METRIC=(
    [rob]=rename_rob_full
    [ldq]=rename_lq_full
    [stq]=rename_sq_full
    [intrf]=rename_reg_full
    [fprf]=rename_reg_full
    [addsched]=iew_iq_full
    [mulsched]=iew_iq_full
    [faddsched]=iew_iq_full
    [fmulsched]=iew_iq_full
    [loadsched]=iew_iq_full
    [storeaddrsched]=iew_iq_full
    [storedatasched]=iew_iq_full
    [mixloadstoresched]=iew_iq_full
)

usage() {
    cat <<'EOF'
Usage:
  tools/run_gem5_asmgen_points.sh [test ...]
  tools/run_gem5_asmgen_points.sh test=low:high[:step] ...

Each x is built and run as a separate binary/range, so stats are attributable
to that x. Raw logs and TSV files are intentionally ignored by git.
EOF
}

parse_range() {
    local spec=$1
    local low high step
    IFS=: read -r low high step <<< "$spec"
    step=${step:-1}
    if [ -z "$low" ] || [ -z "$high" ] || [ -z "$step" ]; then
        return 1
    fi
    RANGE_LOW=$low
    RANGE_HIGH=$high
    RANGE_STEP=$step
}

extract_stat() {
    local file=$1
    local key=$2
    awk -v key="$key" '$1 == key { value = $2 } END { if (value != "") print value; else print 0 }' "$file"
}

metric_value() {
    local metric=$1
    case "$metric" in
        rename_rob_full) echo "$rename_rob_full" ;;
        rename_iq_full) echo "$rename_iq_full" ;;
        rename_lq_full) echo "$rename_lq_full" ;;
        rename_sq_full) echo "$rename_sq_full" ;;
        rename_reg_full) echo "$rename_reg_full" ;;
        iew_iq_full) echo "$iew_iq_full" ;;
        iew_lsq_full) echo "$iew_lsq_full" ;;
        *) echo 0 ;;
    esac
}

if [ "${1:-}" = "--help" ] || [ "${1:-}" = "-h" ]; then
    usage
    exit 0
fi

if [ "$#" -gt 0 ]; then
    TEST_SPECS=("$@")
else
    TEST_SPECS=(addsched mulsched loadsched storeaddrsched storedatasched)
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
    echo "gem5_extra_args: $GEM5_EXTRA_ARGS"
    echo "test_specs: ${TEST_SPECS[*]}"
    echo "prefetch: default on; this script does not pass --no-pf"
    if git -C "$GEM5_ROOT" rev-parse --is-inside-work-tree >/dev/null 2>&1; then
        echo "gem5_branch: $(git -C "$GEM5_ROOT" rev-parse --abbrev-ref HEAD)"
        echo "gem5_commit: $(git -C "$GEM5_ROOT" rev-parse --short=12 HEAD)"
        echo "gem5_dirty: $(git -C "$GEM5_ROOT" status --short | wc -l)"
    fi
} > "$OUT_ROOT/manifest.txt"

printf "test\tx\tstatus\treported_cycles_per_iter\ttarget_metric\ttarget_value\tiew_iq_full\tiew_lsq_full\trename_rob_full\trename_iq_full\trename_lq_full\trename_sq_full\trename_reg_full\trename_stall_reg_full\trename_stall_rob_full\trename_stall_lsq_full\tm5out\tlog\n" > "$OUT_ROOT/points.tsv"

for spec in "${TEST_SPECS[@]}"; do
    test=${spec%%=*}
    if [[ "$spec" == *=* ]]; then
        range=${spec#*=}
    else
        range=${RANGES[$test]:-}
    fi

    if [ -z "$range" ]; then
        echo "No candidate range configured for $test" | tee "$OUT_ROOT/${test}.error.log"
        continue
    fi
    if ! parse_range "$range"; then
        echo "Bad range for $test: $range" | tee "$OUT_ROOT/${test}.error.log"
        continue
    fi

    target_metric=${TARGET_METRIC[$test]:-iew_iq_full}
    test_points="$OUT_ROOT/${test}.points.tsv"
    printf "test\tx\tstatus\treported_cycles_per_iter\ttarget_metric\ttarget_value\tiew_iq_full\tiew_lsq_full\trename_rob_full\trename_iq_full\trename_lq_full\trename_sq_full\trename_reg_full\trename_stall_reg_full\trename_stall_rob_full\trename_stall_lsq_full\tm5out\tlog\n" > "$test_points"

    for ((x = RANGE_LOW; x <= RANGE_HIGH; x += RANGE_STEP)); do
        echo "== build $test x=$x =="
        if ! make -C AsmGen ARCH=riscv TIMING=cycle ONLY="$test" RANGE="$test=$x:$x:1" CROSS_COMPILE_RISCV64="$RISCV_PREFIX" > "$OUT_ROOT/${test}_${x}.build.log" 2>&1; then
            printf "%s\t%s\tbuild_failed\tNA\t%s\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\tNA\t%s\n" \
                "$test" "$x" "$target_metric" "$OUT_ROOT/${test}_${x}.build.log" >> "$test_points"
            continue
        fi

        m5out="/tmp/uarchtest-se-ideal-point-${test}-${x}-${ITER}"
        log="$OUT_ROOT/${test}_${x}.gem5.log"
        echo "== run $test x=$x =="
        timeout "$TIMEOUT" "$GEM5_BIN" \
            -d "$m5out" \
            "$GEM5_CONFIG" \
            --ideal-kmhv3 \
            "${GEM5_EXTRA_ARGS_ARY[@]}" \
            --maxinsts "$MAXINSTS" \
            --cmd="$PWD/AsmGen/generate/clammicrobench_riscv" \
            --options="$test $ITER" > "$log" 2>&1
        status=$?

        reported_val=$(awk -F, '/^[0-9]+,[[:space:]]*[0-9.eE+-]+/ {
            gsub(/[[:space:]]/, "", $2);
            print $2;
            exit;
        }' "$log")
        if [ -z "$reported_val" ]; then
            reported_val="NA"
        fi

        stats="$m5out/stats.txt"
        if [ -f "$stats" ]; then
            iew_iq_full=$(extract_stat "$stats" "system.cpu.iew.iqFullEvents")
            iew_lsq_full=$(extract_stat "$stats" "system.cpu.iew.lsqFullEvents")
            rename_rob_full=$(extract_stat "$stats" "system.cpu.rename.ROBFullEvents")
            rename_iq_full=$(extract_stat "$stats" "system.cpu.rename.IQFullEvents")
            rename_lq_full=$(extract_stat "$stats" "system.cpu.rename.LQFullEvents")
            rename_sq_full=$(extract_stat "$stats" "system.cpu.rename.SQFullEvents")
            rename_reg_full=$(extract_stat "$stats" "system.cpu.rename.fullRegistersEvents")
            rename_stall_reg_full=$(extract_stat "$stats" "system.cpu.rename.stallEvents::RegFull")
            rename_stall_rob_full=$(extract_stat "$stats" "system.cpu.rename.stallEvents::ROBFull")
            rename_stall_lsq_full=$(extract_stat "$stats" "system.cpu.rename.stallEvents::LSQFull")
        else
            iew_iq_full=NA
            iew_lsq_full=NA
            rename_rob_full=NA
            rename_iq_full=NA
            rename_lq_full=NA
            rename_sq_full=NA
            rename_reg_full=NA
            rename_stall_reg_full=NA
            rename_stall_rob_full=NA
            rename_stall_lsq_full=NA
        fi

        target_value=$(metric_value "$target_metric")
        printf "%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\n" \
            "$test" "$x" "$status" "$reported_val" "$target_metric" "$target_value" \
            "$iew_iq_full" "$iew_lsq_full" "$rename_rob_full" "$rename_iq_full" \
            "$rename_lq_full" "$rename_sq_full" "$rename_reg_full" \
            "$rename_stall_reg_full" "$rename_stall_rob_full" "$rename_stall_lsq_full" \
            "$m5out" "$log" >> "$test_points"
    done

    tail -n +2 "$test_points" >> "$OUT_ROOT/points.tsv"

    awk -v test="$test" '
        BEGIN {
            FS = OFS = "\t";
            first = "NA";
            last_zero = "NA";
            first_ge_2x = "NA";
            first_ge_5x = "NA";
            first_ge_10x = "NA";
        }
        NR == 1 { next }
        {
            x = $2;
            v = $6;
            if (v == "NA") next;
            n++;
            values[n] = v + 0;
            xs[n] = x;
            if (v + 0 == 0) last_zero = x;
            if (first == "NA" && v + 0 > 0) first = x;
        }
        END {
            if (n == 0) {
                print test, "NA", "NA", "NA", "NA", "NA", "NA";
                exit;
            }
            base_n = n < 2 ? n : 2;
            for (i = 1; i <= base_n; i++) {
                baseline += values[i];
            }
            baseline /= base_n;
            for (i = base_n + 1; i <= n; i++) {
                if (baseline > 0 && first_ge_2x == "NA" && values[i] >= baseline * 2) {
                    first_ge_2x = xs[i];
                }
                if (baseline > 0 && first_ge_5x == "NA" && values[i] >= baseline * 5) {
                    first_ge_5x = xs[i];
                }
                if (baseline > 0 && first_ge_10x == "NA" && values[i] >= baseline * 10) {
                    first_ge_10x = xs[i];
                }
            }
            printf "%s\t%s\t%s\t%.6f\t%s\t%s\t%s\n", test, last_zero, first, baseline, first_ge_2x, first_ge_5x, first_ge_10x;
        }
    ' "$test_points" >> "$OUT_ROOT/first_nonzero.tmp"
done

{
    printf "test\tlast_zero_x\tfirst_nonzero_x\tbaseline_first2\tfirst_ge_2x\tfirst_ge_5x\tfirst_ge_10x\n"
    cat "$OUT_ROOT/first_nonzero.tmp" 2>/dev/null || true
} > "$OUT_ROOT/summary.tsv"
rm -f "$OUT_ROOT/first_nonzero.tmp"
