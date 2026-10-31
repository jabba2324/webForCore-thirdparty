#!/usr/bin/env bash
# Downloads webForCore's packages from its GitHub release into packages/webforcore.
# Usage: tools/fetch-webforcore.sh [version]   (default: WebForCoreVersion in Directory.Build.props)
set -euo pipefail
cd "$(dirname "$0")/.."
version=${1:-$(sed -n 's#.*<WebForCoreVersion[^>]*>\(.*\)</WebForCoreVersion>.*#\1#p' Directory.Build.props)}
mkdir -p packages/webforcore
gh release download "v$version" --repo jabba2324/webForCore --pattern '*.nupkg' --dir packages/webforcore --clobber
echo "webForCore $version: $(ls packages/webforcore/*."$version".nupkg | wc -l) packages in packages/webforcore"
