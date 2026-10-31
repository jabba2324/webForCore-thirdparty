#!/usr/bin/env bash
# Packs the solution's projects into artifacts/packages (built first with the same -c Release).
# Usage: tools/pack.sh [version]   (default: Version in Directory.Build.props)
set -euo pipefail
cd "$(dirname "$0")/.."
args=(-c Release --no-build -nodeReuse:false)
[ $# -ge 1 ] && args+=(-p:Version="$1")
rm -rf artifacts/packages
dotnet pack webForCore-thirdparty.slnx "${args[@]}"
ls artifacts/packages
