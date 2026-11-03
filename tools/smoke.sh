#!/usr/bin/env bash
# Builds and runs tools/smoke against webForCore's packages (packages/webforcore) and this repository's
# (artifacts/packages), outside the repository's build files, and requests its pages.
# Usage: tools/smoke.sh <this repository's package version> <webForCore version>
set -euo pipefail
root=$(cd "$(dirname "$0")/.." && pwd); version=$1; webforcore=$2
# The package folders as dotnet sees them: on Windows (Git Bash) /d/a/... is D:/a/..., not C:\d\a\...
sources=$(cygpath -m "$root" 2>/dev/null || echo "$root")
base=$(mktemp -d); work=$base/site; mkdir "$work"
# The host's worker stops a moment after the host: on Windows its folder can't be removed until it has.
cleanup() {
  kill ${server:-} 2>/dev/null || true
  for i in $(seq 30); do rm -rf "$base" 2>/dev/null && return 0; sleep 1; done
  rm -rf "$base"
}
trap cleanup EXIT
cp -r "$root/tools/smoke/." "$work"
cd "$work"
cat > nuget.config <<CONFIG
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="thirdparty" value="$sources/artifacts/packages" />
    <add key="webforcore" value="$sources/packages/webforcore" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="thirdparty">
      <package pattern="webForCore.WebActivatorEx" />
      <package pattern="webForCore.WebActivator" />
      <package pattern="webForCore.AjaxControlToolkit*" />
      <package pattern="webForCore.AutoMapperLegacy" />
    </packageSource>
    <packageSource key="webforcore">
      <package pattern="webForCore" />
      <package pattern="webForCore.*" />
      <package pattern="drawingForCore" />
      <package pattern="drawingForCore.*" />
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
# Under webforcore-host, which the webForCore package puts in the output (from webForCore 0.7.0), as an application runs.
dotnet bin_dotnet/webforcore-host/webforcore-host.dll --urls http://127.0.0.1:5197 > server.log 2>&1 &
server=$!
# -f: the host answers 503 until its worker serves.
for i in $(seq 120); do curl -sf -o /dev/null http://127.0.0.1:5197/Plain && break; sleep 1; done
# Unity, registered by a WebActivatorEx PreApplicationStartMethod, constructs the controller; a PostApplicationStartMethod
# ran (webForCore issue #21: with the original package no start method runs).
home=$(curl -s http://127.0.0.1:5197/)
echo "$home" | grep -q "hello from Unity, rendered by Razor" || { echo "/: unexpected response"; echo "$home" | head -20; fail=1; }
echo "$home" | grep -q "post-start: True" || { echo "/: PostApplicationStartMethod didn't run"; fail=1; }
# WebActivator 1.x's pre-start and post-start methods ran (the original package's never do, as for WebActivatorEx).
curl -s http://127.0.0.1:5197/Plain | grep -q "WebActivator 1.x: pre-start True, post-start True" || { echo "/Plain: WebActivator 1.x start methods didn't run"; curl -s http://127.0.0.1:5197/Plain | grep -i 'WebActivator\|<title>' | head -3; fail=1; }
# AutoMapper 3.x's static API maps (webForCore.AutoMapperLegacy, on AutoMapper 12): CreateMap in Application_Start, a
# ValueResolver, ResolveUsing over the context with the Map call's items, and ConvertUsing with a statement lambda.
mapped=$(curl -s http://127.0.0.1:5197/Plain/Mapped)
echo "$mapped" | grep -q "AutoMapperLegacy: Dune by Herbert; Dune (2 tags); shelf B2; tags classic,sf" || { echo "/Plain/Mapped: unexpected response"; echo "$mapped" | head -20; fail=1; }
# An AjaxControlToolkit extender renders its client behaviour.
calendar=$(curl -s http://127.0.0.1:5197/Calendar.aspx)
echo "$calendar" | grep -q "Sys.Extended.UI.CalendarBehavior" || { echo "/Calendar.aspx: unexpected response"; echo "$calendar" | head -30; fail=1; }
if [ $fail -ne 0 ]; then cat server.log; exit 1; fi
echo "Smoke test passed ($version on webForCore $webforcore)."
