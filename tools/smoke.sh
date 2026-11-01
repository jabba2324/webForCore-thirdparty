#!/usr/bin/env bash
# Builds and runs tools/smoke against webForCore's packages (packages/webforcore) and this repository's
# (artifacts/packages), outside the repository's build files, and requests its pages.
# Usage: tools/smoke.sh <this repository's package version> <webForCore version>
set -euo pipefail
root=$(cd "$(dirname "$0")/.." && pwd); version=$1; webforcore=$2
base=$(mktemp -d); work=$base/site; mkdir "$work"
trap 'kill $server 2>/dev/null || true; rm -rf "$base"' EXIT
cp -r "$root/tools/smoke/." "$work"
cd "$work"
cat > nuget.config <<CONFIG
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="thirdparty" value="$root/artifacts/packages" />
    <add key="webforcore" value="$root/packages/webforcore" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="thirdparty">
      <package pattern="webForCore.WebActivatorEx" />
      <package pattern="webForCore.WebActivator" />
      <package pattern="webForCore.AjaxControlToolkit*" />
    </packageSource>
    <packageSource key="webforcore">
      <package pattern="webForCore" />
      <package pattern="webForCore.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
CONFIG
dotnet build Smoke.csproj -c Release -p:WebForCoreVersion="$webforcore" -p:ThirdPartyVersion="$version" -nodeReuse:false
fail=0
# The output has this repository's WebActivatorEx, not the .NET Framework one that Unity.Mvc brings.
if grep -q -E '"lib/net4[0-9]*/WebActivatorEx\.dll"' bin_dotnet/Smoke.deps.json; then
  echo "deps.json: the original WebActivatorEx replaced webForCore.WebActivatorEx"; fail=1
fi
if grep -q -E '"lib/net4[0-9]*/WebActivator\.dll"' bin_dotnet/Smoke.deps.json; then
  echo "deps.json: the original WebActivator replaced webForCore.WebActivator"; fail=1
fi
dotnet bin_dotnet/Smoke.dll --urls http://127.0.0.1:5197 > server.log 2>&1 &
server=$!
for i in $(seq 120); do curl -s -o /dev/null http://127.0.0.1:5197/Plain && break; sleep 1; done
# Unity, registered by a WebActivatorEx PreApplicationStartMethod, constructs the controller; a PostApplicationStartMethod
# ran (webForCore issue #21: with the original package no start method runs).
home=$(curl -s http://127.0.0.1:5197/)
echo "$home" | grep -q "hello from Unity, rendered by Razor" || { echo "/: unexpected response"; echo "$home" | head -20; fail=1; }
echo "$home" | grep -q "post-start: True" || { echo "/: PostApplicationStartMethod didn't run"; fail=1; }
# WebActivator 1.x's pre-start and post-start methods ran (the original package's never do, as for WebActivatorEx).
curl -s http://127.0.0.1:5197/Plain | grep -q "WebActivator 1.x: pre-start True, post-start True" || { echo "/Plain: WebActivator 1.x start methods didn't run"; curl -s http://127.0.0.1:5197/Plain | grep -i 'WebActivator\|<title>' | head -3; fail=1; }
# An AjaxControlToolkit extender renders its client behaviour.
calendar=$(curl -s http://127.0.0.1:5197/Calendar.aspx)
echo "$calendar" | grep -q "Sys.Extended.UI.CalendarBehavior" || { echo "/Calendar.aspx: unexpected response"; echo "$calendar" | head -30; fail=1; }
if [ $fail -ne 0 ]; then cat server.log; exit 1; fi
echo "Smoke test passed ($version on webForCore $webforcore)."
