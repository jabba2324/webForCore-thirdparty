# webForCore-thirdparty

Libraries that aren't Microsoft's, built for applications on [webForCore](https://github.com/jabba2324/webForCore).

webForCore runs ASP.NET 4.x applications on .NET 10 LTS with Microsoft's own libraries, and ships **only Microsoft
assemblies**. Applications also use third-party libraries that need changes to run on .NET or under webForCore.
This repository builds those against webForCore's released packages to help the
[smoke test](https://github.com/jabba2324/webForCore-smoke-test) migrate real applications that use them. It isn't
part of webForCore, and webForCore doesn't depend on it: webForCore's own documentation gives the workaround for each
(for WebActivatorEx, calling the start methods from `Application_Start`).

| Package | Assembly | What it is | Why it is needed |
| --- | --- | --- | --- |
| `webForCore.WebActivatorEx` | WebActivatorEx 2.0.0.0 | WebActivatorEx's pre-start, post-start and shutdown attributes and `ActivationManager`, written from its documented API (MIT) | The original package finds the attributes with `Assembly.LoadFrom`, which on .NET loads second copies of the application's assemblies into the default load context, so no start method runs (Unity.Mvc, Ninject.MVC, StructureMap.MVC; [webForCore#21](https://github.com/jabba2324/webForCore/issues/21)). This one looks in the application's context. `webactivator:assembliesToScan` isn't read: the assemblies that reference WebActivatorEx are scanned. |
| `webForCore.WebActivator` | WebActivator 1.5.0.0 | WebActivator 1.x (the package before WebActivatorEx), written from its documented API (MIT) | The same problem as WebActivatorEx: the original finds the attributes with `Assembly.LoadFrom`, so no start method runs. MVC 3-era packages bring it (Ninject.MVC3 2.x; Subtext). The original isn't strong-named, and neither is this one. |
| `webForCore.AjaxControlToolkit` (+ `.StaticResources`, `.HtmlEditor.Sanitizer`) | AjaxControlToolkit 20.1.0.0 | The [AJAX Control Toolkit](https://github.com/DevExpress/AjaxControlToolkit) 20.1 (BSD 3-Clause), built for .NET 10 against webForCore | The original package is built for .NET Framework. Designers are .NET Framework only. |

Each package replaces the original package of the same assembly when both are in a build (for example
WebActivatorEx, which Unity.Mvc brings): its `buildTransitive` targets make it win the SDK's package conflict
resolution at any version of the original.

## Using the packages

They aren't on nuget.org. Since 0.5.0 there is a feed on GitHub Pages,
https://jabba2324.github.io/webForCore-thirdparty/index.json, rebuilt after each release from the latest five
releases (they are still attached to the [GitHub releases](https://github.com/jabba2324/webForCore-thirdparty/releases)).
Add it to your `nuget.config` next to webForCore's and drawingForCore's feeds (see webForCore's `docs/MIGRATING.md`):

```xml
<add key="webForCore" value="https://jabba2324.github.io/webForCore/index.json" />
<add key="drawingForCore" value="https://jabba2324.github.io/drawingForCore/index.json" />
<add key="webForCore-thirdparty" value="https://jabba2324.github.io/webForCore-thirdparty/index.json" />
```

and reference them next to `webForCore`:

```xml
<PackageReference Include="webForCore" Version="0.7.0" />
<PackageReference Include="webForCore.WebActivatorEx" Version="0.5.0" />
```

Each release says which webForCore version it was built and tested against; later webForCore versions satisfy its
dependencies. From 0.5.0 they target .NET 10 (`net10.0`) and need webForCore 0.7.0 or later; 0.4.x target
.NET 11 RC and webForCore 0.3.0-0.6.0.

## Building

```bash
tools/fetch-webforcore.sh                       # webForCore's and drawingForCore's packages into packages/webforcore
dotnet build webForCore-thirdparty.slnx -c Release
tools/pack.sh                                   # artifacts/packages
tools/smoke.sh 0.5.0 0.7.0                      # tools/smoke on both repositories' packages, under webforcore-host
```

`tools/smoke` is an MVC 5 and Web Forms application with Unity.Mvc and the original WebActivatorEx package, as
applications have them: Unity must be started by a WebActivatorEx start method, the post-start method must run, the
output must not contain the original WebActivatorEx, and an AjaxControlToolkit extender must render. CI
(`.github/workflows/ci.yml`) runs it on every push and pull request, on Linux and on Windows; a `v<Version>` tag
publishes a GitHub release and rebuilds the feed (`.github/workflows/feed.yml`). How changes are made:
[CONTRIBUTING.md](CONTRIBUTING.md), which holds this repository to webForCore's rules.

## Identities

The assemblies keep their original names and versions so applications and packages built against the originals bind
to them. AjaxControlToolkit is signed with its own published key (`src/AjaxControlToolkit/AjaxKey.snk`).
WebActivatorEx carries the original's public key (token `7b26dc2a43f6a0d4`) and is delay signed with it
(`WebActivatorEx.public.snk`, the public key only): it is **not** signed by WebActivatorEx's author. .NET doesn't
verify strong-name signatures.

Licences: see [LICENSE](LICENSE) and [NOTICE](NOTICE).
