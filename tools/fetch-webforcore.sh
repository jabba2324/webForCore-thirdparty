#!/usr/bin/env bash
# Downloads webForCore's packages from its GitHub release into packages/webforcore, with drawingForCore's (System.Drawing,
# which the webForCore package depends on from 0.7.0).
# Usage: tools/fetch-webforcore.sh [version] [drawingForCore version]
#        (defaults: WebForCoreVersion and DrawingForCoreVersion in Directory.Build.props)
set -euo pipefail
cd "$(dirname "$0")/.."
version=${1:-$(sed -n 's#.*<WebForCoreVersion[^>]*>\(.*\)</WebForCoreVersion>.*#\1#p' Directory.Build.props)}
drawing=${2:-$(sed -n 's#.*<DrawingForCoreVersion[^>]*>\(.*\)</DrawingForCoreVersion>.*#\1#p' Directory.Build.props)}
mkdir -p packages/webforcore
gh release download "v$version" --repo xforcore/webForCore --pattern '*.nupkg' --dir packages/webforcore --clobber
gh release download "v$drawing" --repo xforcore/drawingForCore --pattern '*.nupkg' --dir packages/webforcore --clobber
echo "webForCore $version: $(ls packages/webforcore/*."$version".nupkg | wc -l) packages, drawingForCore $drawing: $(ls packages/webforcore/drawingForCore*."$drawing".nupkg | wc -l), in packages/webforcore"
