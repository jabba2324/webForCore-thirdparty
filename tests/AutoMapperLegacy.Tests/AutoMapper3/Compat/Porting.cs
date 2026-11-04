// AutoMapper 3.3.1's unit tests (src/UnitTests at tag v3.3.1 of https://github.com/LuckyPennySoftware/AutoMapper,
// Copyright (c) 2010 Jimmy Bogard, MIT License; see NOTICE), ported as a conformance suite for
// webForCore.AutoMapperLegacy (issue #9). Each file keeps its path and its code, with these mechanical changes:
//
//   * The namespaces AutoMapper.UnitTests.* (and AutoMapper.Tests) are AutoMapper3.UnitTests.*, and the files have
//     `using AutoMapper;`. Code inside a namespace called AutoMapper finds AutoMapper 12's own types (its instance
//     Mapper and its Profile) before any using alias: an application whose code is in such a namespace has to rename it,
//     as these tests did.
//   * ITypeConverter<,> and IMappingAction<,> are written AutoMapperLegacy.ITypeConverter<,> and
//     AutoMapperLegacy.IMappingAction<,>: AutoMapper 12 has interfaces of the same names (README, "Differences").
//   * The Should library they used is replaced by Compat/Should.cs, and xUnit 1 by xUnit 2 (Compat/Fact.cs).
//   * Rhino Mocks, which only AutoMapperSpecBase referenced, is left out (#if RHINO_MOCKS).
//
// Everything else that differs is a test that is skipped, not deleted: [PortSkip(Skips.X)] on the test or on its class (or
// an enclosing class), with one of the reasons below, or, for the few that don't compile against the facade, an
// #if on the reason's symbol (#if AM3_<REASON>) around the test.
namespace AutoMapper3.UnitTests
{
    /// <summary>Why a ported test is skipped. Each reason is one kind of difference.</summary>
    public static class Skips
    {
        public const string Formatters =
            "Formatters (AddFormatter, SkipFormatter, ForSourceType, IValueFormatter) were removed in AutoMapper 4 and AutoMapperLegacy doesn't provide them.";

        public const string Engine =
            "Tests AutoMapper 3.x's engine internals (TypeMapFactory, MappingEngine, IObjectMapper, DelegateFactory, TypeInfo, PropertyMap, ResolutionContext construction), which AutoMapper 12 replaced.";

        public const string DataReader =
            "IDataReader and DataTable mapping was removed in AutoMapper 4 (it was a separate package afterwards).";

        public const string NoDynamicMaps =
            "AutoMapper 12 doesn't create maps implicitly: a member or element type without a CreateMap isn't mapped by DynamicMap, which only creates the top-level map.";

        public const string Validation =
            "AutoMapper 12's configuration validation differs from 3.x's (it checks constructor parameters, ignored members and resolvers differently).";

        public const string NullCollections =
            "AutoMapper 12 maps a null source collection to an empty one (AllowNullCollections = false) and keeps the destination's collection where 3.x replaced it.";

        public const string Behaviour =
            "AutoMapper 12 maps this case differently from AutoMapper 3.x; the facade can't change how AutoMapper 12 maps.";

        public const string Projection =
            "AutoMapper 12's LINQ projection differs from 3.x's here (what it can translate, null handling, or explicit expansion).";

        public const string Removed =
            "Uses a member AutoMapper 12 removed and the facade doesn't provide (see the README).";

        public const string Platform =
            "Platform-specific (.NET Framework, Silverlight or WinRT behaviour, or timing on a particular machine).";
    }
}
