# Contributing to webForCore-thirdparty

webForCore-thirdparty builds third-party libraries that applications on [webForCore](https://github.com/xforcore/webForCore)
use, where the original doesn't work on .NET or under webForCore. It holds itself to webForCore's rules
([webForCore's CONTRIBUTING.md](https://github.com/xforcore/webForCore/blob/webforcore-lift/CONTRIBUTING.md)), adapted
to a repository of other people's libraries. A change is welcome when it lets a real application run as it did on
.NET Framework 4.8 and IIS, and when it can prove it with a test.

## The principles

1. **Only what applications need.** A library is here because a real application uses it and the original package
   doesn't work on webForCore (for WebActivatorEx: no start method runs, webForCore#21). webForCore doesn't depend on
   this repository, and its documentation gives a workaround without it; a problem that is webForCore's is fixed in
   webForCore, not worked around here.

2. **Every change comes with a test.** `tools/smoke` is an application with the original packages, as applications
   have them, that checks the behaviour each package exists for (start methods run, an AjaxControlToolkit extender
   renders, the original assembly isn't in the output). A fix extends it so that it fails without the fix; say in the
   pull request that it failed there.

3. **Every change names a real application it affects.** Say which application hit the problem, what it did there and
   what its user saw. The [webForCore smoke test](https://github.com/xforcore/webForCore-smoke-test) migrates
   open-source applications and is the usual source.

4. **Avoid changing the original library's code.** AjaxControlToolkit is DevExpress's code, through WebFormsForCore's
   port; its value is that it is the library applications were written against. Keep a change as small as it can be
   (an `#if`, a guarded branch), mark it with a `// webForCore:` comment saying why and the issue number, and say in
   NOTICE what was changed. WebActivatorEx and WebActivator are written from their documented API: match the
   original's documented behaviour, and say in a comment where they differ and why.

5. **Keep the identities.** The assemblies keep the originals' names, versions and public keys, so applications and
   packages built against the originals bind to them (README, "Identities"). Each package's `buildTransitive` targets
   make it win over the original package at any version.

## How this repository's own code is written

These are webForCore's rules for its own code (its CONTRIBUTING.md, 6–9), for the code written here: WebActivatorEx,
WebActivator, the changes to AjaxControlToolkit, the scripts and `tools/smoke`. They don't ask for changes to the
original library's code beyond principle 4.

6. **No reflection into .NET's internals.** Don't read, write or call non-public members of the runtime, ASP.NET Core,
   webForCore or any other library, and don't depend on their private names or layout. Reflection over the
   *application's* own types, to do what the original did on .NET Framework (find the start methods its attributes
   name), is fine; use public members where there are any, and say in a comment why reflection is needed.

7. **No unsafe code and no GC tricks.** No `unsafe` blocks or pointers, no `Unsafe`/`MemoryMarshal`
   reinterpretation, no P/Invoke, and nothing that steers the garbage collector: no `GC.Collect`,
   `GC.WaitForPendingFinalizers` or `GC.AddMemoryPressure`, no `GCHandle`s, no polling weak references.

8. **Slow, secure and stable beats fast.** Don't optimise before an application shows a problem with a measurement.
   Choose the simplest design that is clearly correct over a faster one that is harder to reason about.

9. **Programs and tools are .NET.** Everything this repository builds, and every tool it uses, is written in C# as a
   project or as a file-based app run with `dotnet run tool.cs`. No Python, Node.js, Ruby, Perl or other
   general-purpose language. Shell scripts (bash, PowerShell) and CI YAML are fine as glue that calls dotnet, git, gh
   and curl. Anything that parses data or makes decisions belongs in C#. Front-end code is fine: JavaScript,
   TypeScript, CSS, HTML and anything else that runs in the browser (such as AjaxControlToolkit's client scripts and
   styles) is part of the product, not tooling.

No code here broke these rules when they were added (3 Nov 2026).

## Pull requests

- One problem per commit, with the application that hit it and `Fixes #<issue>` in the message where there is one.
- The pull request lists, for each change: the problem, the application, the fix, the test, and whether the original
  library's code changed (and where that is recorded).
- CI (`.github/workflows/ci.yml`) builds against webForCore's released packages, packs the packages and runs the smoke
  test, on Linux and on Windows; both must pass.

## Releases

1. Set `<Version>` (and `<WebForCoreVersion>`, `<DrawingForCoreVersion>` when they move) in `Directory.Build.props`,
   and write `docs/release-notes/<version>.md`.
2. Tag the merged commit `v<version>`. CI builds and tests the tag on both platforms, creates the GitHub release with
   the packages, and rebuilds the NuGet feed (https://xforcore.github.io/webForCore-thirdparty/index.json).

Licences: this repository's own code is MIT; the libraries keep their licences (LICENSE, NOTICE). Contributions are
accepted under the licence of the file they change.
