# webForCore.AutoMapperLegacy

AutoMapper 3.x and 4.x's **static API** (`Mapper.CreateMap`, `Mapper.Map`, `Profile.Configure`, `ValueResolver<,>`,
`TypeConverter<,>`, `ResolveUsing`, `Project().To<T>()`, ...) on top of **AutoMapper 12–14**, so that an application
written against it recompiles on .NET with its mapping code unchanged.

AutoMapper before 12 doesn't run on .NET: configuring a map throws (`The type or method has 3 generic parameter(s), but
1 generic argument(s) were provided`) while it scans `System.Linq.Enumerable`'s methods. Moving to AutoMapper 12 from the
static API means rewriting every mapping class. This package is that static API, built on AutoMapper 12's instance API.
It isn't AutoMapper, isn't affiliated with it, and doesn't replace its assembly: it has its own name and namespace
(`AutoMapperLegacy`) and depends on AutoMapper.

## Dependency, licence and security

- It depends on **AutoMapper `[12.0.1, 15)`** from nuget.org and is built against 12.0.1. 12.0.1 to 14 are under the
  MIT licence; **AutoMapper 15 and later are under the RPL 1.5 or a commercial licence**, which is why the range stops
  before 15. Tests run with 12.0.1, 13.0.1 and 14.0.0.
- **AutoMapper 12–14 carry [GHSA-rvv3-g6hj-g44x](https://github.com/advisories/GHSA-rvv3-g6hj-g44x)**, a denial of
  service through deep recursion when mapping deeply nested object graphs, fixed only in 15.1.1. AutoMapper 3.x and
  4.x have it too. This package doesn't change AutoMapper's code and doesn't fix it; NuGet audit reports it (NU1903) in
  every project that references this package. Don't map untrusted, arbitrarily deep input.
- This package is MIT. Its API follows AutoMapper 3.3.1's and 4.2.1's, and parts are adapted from them (Copyright (c)
  2010 Jimmy Bogard, MIT; see NOTICE).

## Using it

1. Replace the `AutoMapper` 3.x or 4.x package with this one:

   ```xml
   <PackageReference Include="webForCore.AutoMapperLegacy" Version="0.6.0" />
   ```

2. Add, once per project:

   ```csharp
   global using AutoMapperLegacy;
   global using Mapper = AutoMapperLegacy.Mapper;    // AutoMapper 12 has its own (instance) AutoMapper.Mapper
   global using Profile = AutoMapperLegacy.Profile;  // only if the project has 3.x profiles (Configure())
   ```

   The files keep their `using AutoMapper;`: AutoMapper 12's `IMappingExpression<,>`, `ResolutionContext`, `TypeMap`
   and the rest are the types the code names, and the facade adds 3.x's members to them.

3. The project needs C# 14, the default for `net10.0` (an old `<LangVersion>` from the .NET Framework project has to
   go): 3.x's `ResolutionContext.SourceValue`, `Options` and the like are extension properties.

Umbraco 7.15's Umbraco.Core and Umbraco.Web (AutoMapper 3.3.1, 83 files that use it) compile with these steps, one
more alias and the `ITypeConverter<,>` change below in 2 files.

### What an application may still have to change

- **A class that implements 3.x's `ITypeConverter<,>` or `IMappingAction<,>` directly** names it
  `AutoMapperLegacy.ITypeConverter<,>` (or `AutoMapperLegacy.IMappingAction<,>`). AutoMapper 12 has interfaces of the
  same names with other methods, and C# can't alias an open generic type. Classes derived from `TypeConverter<,>` or
  `ValueResolver<,>` need nothing.
- **`using AutoMapper;` inside a `namespace` block**, or code in a namespace called `AutoMapper...`, finds AutoMapper
  12's `Mapper` and `Profile` before the global aliases: add the aliases at that level too, or move the using out.
  `AutoMapper.Mapper.Map(...)` written out in full needs to become `Mapper.Map(...)`.
- **An application type with the name of a type AutoMapper added after 3.x** (`MapperConfiguration`, `IMapper`,
  `IMapperConfigurationExpression`, ...) is ambiguous in files with `using AutoMapper;`: alias it
  (Umbraco 7: `global using MapperConfiguration = Umbraco.Core.Models.Mapping.MapperConfiguration;`). For the same
  reason this package doesn't define 4.x's `IMapperConfiguration` (Umbraco has its own): `Mapper.Initialize` takes
  3.x's `IConfiguration`.
- Formatters (`AddFormatter`, `SkipFormatter`, `ForSourceType`, `IValueFormatter`), removed in AutoMapper 4, aren't
  provided. Nor are 3.x's engine types (`MappingEngine`, `ConfigurationStore`, `TypeMapFactory`, `IObjectMapper`,
  `MapperRegistry`, `PlatformAdapter`), `IDataReader` mapping, the per-call options `CreateMissingTypeMaps` and
  `DisableCache`, or `WithProfile` (it throws: create the map in the profile).

## What it provides

- **The static `Mapper`:** `Initialize`, `CreateMap` (generic and by `Type`, with `MemberList`), `CreateProfile`,
  `AddProfile` (3.x profiles and AutoMapper 12 profiles), the `Map` overloads (with options and `Items`),
  `DynamicMap`, `AssertConfigurationIsValid` (all, by type map, by profile), `FindTypeMapFor`, `GetAllTypeMaps`, `Reset`,
  `Configuration`, `ConfigurationProvider` and 4.x's `Instance` (AutoMapper 12's), `Engine` (3.x's `IMappingEngine`),
  `AllowNullDestinationValues`, `AddGlobalIgnore`.
- **`IConfiguration`** (`Mapper.Initialize(cfg => ...)`) and **`Profile`** (`Configure()`, or `CreateMap` in the
  constructor as 4.x allowed): maps, `ConstructServicesUsing`, `AllowNullDestinationValues`, `AllowNullCollections`,
  4.x's `CreateMissingTypeMaps`, naming conventions, `RecognizePrefixes`/`Postfixes`/`DestinationPrefixes`/
  `DestinationPostfixes`, `RecognizeAlias`, `ReplaceMemberName`, `ClearPrefixes`, `AddGlobalIgnore`,
  `DisableConstructorMapping`, `IncludeSourceExtensionMethods(Assembly)`, `ShouldMapProperty`/`Field`, `ForAllMaps`.
- **The map** (`LegacyMappingExpression<,>`, which implements AutoMapper 12's `IMappingExpression<,>`): 3.x's
  `ForMember`, `ForSourceMember` (with `Ignore()`), `ForAllMembers`, `ConvertUsing` (a function, a 3.x or 12 converter
  instance or type), `ConstructUsing` (a function of the source or of the context), `ConstructProjectionUsing`,
  `ProjectUsing`, `BeforeMap`/`AfterMap` (functions or 3.x or 12 action types), `Include`, `IncludeBase`, `ReverseMap`,
  `Substitute`, `MaxDepth`, `ConstructUsingServiceLocator`, `IgnoreAllPropertiesWithAnInaccessibleSetter`,
  `IgnoreAllSourcePropertiesWithAnInaccessibleSetter`, `As`, `TypeMap`; and AutoMapper 12's own members. Its
  `ConvertUsing` and `ConstructUsing` take functions, as 3.x's did, so statement lambdas compile (AutoMapper 12's take
  expression trees).
- **Member options:** `MapFrom` (expression or name), `ResolveUsing` (a resolver type, `ResolveUsing<T>()`, an
  instance, a function of the source or of the `ResolutionResult`) with `FromMember` and `ConstructedBy`, `UseValue`,
  `Ignore`, `UseDestinationValue`, `NullSubstitute`, `Condition` and `PreCondition` (of the source or of the context),
  `SetMappingOrder`, `ExplicitExpansion`; and AutoMapper 12's. On AutoMapper 12's own member configuration (an
  application's helper over `IMappingExpression<,>`), the same are extension methods, except `ResolveUsing<T>()`.
- **Base classes and types:** `ValueResolver<,>` (`ResolveCore`), `IValueResolver`, `ResolutionResult`,
  `TypeConverter<,>` (`ConvertCore`), `ITypeConverter<,>`, `IMappingAction<,>`, `IMappingEngine`, `[IgnoreMap]`.
- **`ResolutionContext`** (AutoMapper 12's, with 3.x's members as extension properties): `SourceValue`,
  `DestinationValue`, `SourceType`, `DestinationType`, `MemberName`, `IsSourceValueNull`, `Options.Items`, `Engine`.
- **Projection:** `Project().To<T>()` (with parameters and members to expand), `Project(engine)`, 4.x's
  `ProjectTo<T>()` without a configuration, `Mapper.Engine.CreateMapExpression<TSource, TDestination>()`, and
  `TypeMap.GetPropertyMaps()`.

## How it behaves, and where it differs from 3.x and 4.x

The mapping itself is AutoMapper 12's. The facade records the configuration and builds an AutoMapper 12
`MapperConfiguration` from it; it can't make AutoMapper 12 map as 3.x did where the two differ.

- **Maps are built on first use.** Registrations are recorded at any time; the first `Map` after one builds a
  configuration and mapper from everything registered (and later registrations rebuild it), as 3.x let maps be added
  until they were used. So configuration errors (a `ForMember` on a nested member, a missing included map) appear at
  the first `Map` or `AssertConfigurationIsValid`, not at `CreateMap` or `Initialize`. The lambdas given to `ForMember`
  run again at each build. `CreateMap` for types that already have a map in the same profile adds to that map, as in
  3.x. `Mapper.Instance` is the mapper of the moment: keep `Mapper` rather than the instance.
- **Thread safety:** registrations and builds take one lock; mapping doesn't (AutoMapper 12's mapper is thread-safe).
- **`ResolutionContext`:** AutoMapper 12's context doesn't carry the value being mapped. `SourceValue`,
  `DestinationValue`, `SourceType`, `DestinationType` and `MemberName` are available while the facade runs 3.x code (a
  converter, a resolver, a condition or a `ConstructUsing` over the context), on that thread, and throw anywhere else.
  `Parent`, `TypeMap`, `PropertyMap`, `ArrayIndex` and `InstanceCache` aren't available. `Options.Items` are the `Map`
  call's items; the facade's `Map` always passes options, so they exist. A `Map` call's `ConstructServicesUsing` applies
  to the resolvers and converters of that call.
- **Conditions:** `Condition(Func<TSource, bool>)` is checked before the value is resolved (AutoMapper 12's
  `PreCondition`), which gives 3.x's result (3.x resolved first but only threw a resolver's exception if the condition
  held). `Condition(Func<ResolutionContext, bool>)` sees the resolved value as `SourceValue`, as in 3.x, but a resolver
  that throws throws even when the condition is false.
- **Resolvers** are constructed for each value, as in 3.x: by `ConstructedBy`, `ConstructServicesUsing`, or their
  constructor. Their result is mapped to the member's type when it isn't one.
- **No implicit maps.** AutoMapper 12 maps only between types with a map. `DynamicMap` (and 4.x's
  `CreateMissingTypeMaps`, for `Map`) creates the map between the two types, and between the types of their members of
  the same name and the elements of their collections, when those are the application's classes.
- **`ConvertUsing(Func<ResolutionContext, TDestination>)`** (4.x) is an extension method, so that 3.x's
  `ConvertUsing(x => Convert.ToInt32(x))` still binds to the source overload. Likewise `ConstructUsing` over the context:
  `ConstructUsing(x => new Foo())`, ambiguous in 3.x, takes the source.
- **Validation** is AutoMapper 12's, which is stricter (constructor parameters, members of included maps) and reports
  the same type map in two profiles.
- **Null collections** map to empty collections unless `AllowNullCollections` is set, as in 4.x and 12.
- **Interface destinations** get a proxy (AutoMapper 12's `AsProxy()`, which the facade sets), as in 3.x.
- **`[IgnoreMap]`** on a destination member ignores it in every map to its class, and on a source member in maps that
  would map it by name (a `ReverseMap`).
- AutoMapper 12 no longer maps expression trees (`Map<Expression<Func<A, bool>>, Expression<Func<B, bool>>>`; see
  AutoMapper.Extensions.ExpressionMapping), with `System.ComponentModel` type converters, `IDataReader`s or non-generic
  dictionaries, and differs in places on inheritance (`Include`), optional constructor parameters and `NullSubstitute`
  for a member of another type. AutoMapper 3.3.1's own tests that cover these are skipped in this package's tests, with
  the reason (`tests/AutoMapperLegacy.Tests/AutoMapper3`).
- `ExplicitExpansion()` became `ExplicitExpansion(bool)` in AutoMapper 13: with 13 and 14 the facade calls the public
  member by reflection, the one place it does.
- **Binary compatibility** with libraries built against AutoMapper 3.x or 4.x isn't a goal: they need recompiling.

## Tests

`tests/AutoMapperLegacy.Tests` has the facade's tests, written as 3.x code is, and AutoMapper 3.3.1's own unit tests
ported as a conformance suite (`AutoMapper3/`, with `Compat/Porting.cs` describing the port). The repository's CI runs
them with AutoMapper 12.0.1 and 14.0.0, and `tools/smoke` maps with the static API under webForCore.
