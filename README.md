# webForCore-thirdparty

Libraries that aren't Microsoft's, built for applications on [webForCore](https://github.com/jabba2324/webForCore).

webForCore runs ASP.NET 4.x applications on .NET 11 with Microsoft's own libraries, and ships **only Microsoft
assemblies**. Applications also use third-party libraries that need changes to run on .NET or under webForCore's
PseudoAppDomain; those live here, built against webForCore's released packages, with their own releases.

| Package | Assembly | What it is | Why it is needed |
| --- | --- | --- | --- |
| `webForCore.WebActivatorEx` | WebActivatorEx 2.0.0.0 | WebActivatorEx's pre-start, post-start and shutdown attributes and `ActivationManager`, written from its documented API (MIT) | The original package finds the attributes with `Assembly.LoadFrom`, which on .NET loads second copies of the application's assemblies into the default load context, so no start method runs (Unity.Mvc, Ninject.MVC, StructureMap.MVC; [webForCore#21](https://github.com/jabba2324/webForCore/issues/21)). This one looks in the application's context. `webactivator:assembliesToScan` isn't read: the assemblies that reference WebActivatorEx are scanned. |
| `webForCore.AjaxControlToolkit` (+ `.StaticResources`, `.HtmlEditor.Sanitizer`) | AjaxControlToolkit 20.1.0.0 | The [AJAX Control Toolkit](https://github.com/DevExpress/AjaxControlToolkit) 20.1 (BSD 3-Clause), built for .NET 11 against webForCore | The original package is built for .NET Framework. Designers are .NET Framework only. |

Each package replaces the original package of the same assembly when both are in a build (for example
WebActivatorEx, which Unity.Mvc brings): its `buildTransitive` targets make it win the SDK's package conflict
resolution at any version of the original.

## Using the packages

They aren't on nuget.org. Download them from a [release](https://github.com/jabba2324/webForCore-thirdparty/releases)
into the folder that holds webForCore's packages, which your `nuget.config` already lists as a package source (see
webForCore's `docs/MIGRATING.md`), and reference them next to `webForCore`:

```bash
gh release download --repo jabba2324/webForCore-thirdparty --pattern '*.nupkg' --dir packages
```

```xml
<PackageReference Include="webForCore" Version="0.4.0" />
<PackageReference Include="webForCore.WebActivatorEx" Version="0.4.0" />
```

Each release says which webForCore version it was built and tested against; later webForCore versions satisfy its
dependencies.

## Building

```bash
tools/fetch-webforcore.sh                       # webForCore's packages (WebForCoreVersion) into packages/webforcore
dotnet build webForCore-thirdparty.slnx -c Release
tools/pack.sh                                   # artifacts/packages
tools/smoke.sh 0.4.0 0.3.0                      # tools/smoke on both repositories' packages
```

`tools/smoke` is an MVC 5 and Web Forms application with Unity.Mvc and the original WebActivatorEx package, as
applications have them: Unity must be started by a WebActivatorEx start method, the post-start method must run, the
output must not contain the original WebActivatorEx, and an AjaxControlToolkit extender must render. CI
(`.github/workflows/ci.yml`) runs it on every push and pull request; a `v<Version>` tag publishes a GitHub release.

## Identities

The assemblies keep their original names and versions so applications and packages built against the originals bind
to them. AjaxControlToolkit is signed with its own published key (`src/AjaxControlToolkit/AjaxKey.snk`).
WebActivatorEx carries the original's public key (token `7b26dc2a43f6a0d4`) and is delay signed with it
(`WebActivatorEx.public.snk`, the public key only): it is **not** signed by WebActivatorEx's author. .NET doesn't
verify strong-name signatures.

Licences: see [LICENSE](LICENSE) and [NOTICE](NOTICE).
