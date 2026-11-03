// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/ReadOnlyCollectionMappingBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    // Bug #511
    // https://github.com/AutoMapper/AutoMapper/issues/511
    public class ReadOnlyCollectionMappingBug
    {
        class Source { public int X { get; set; } }
        class Target { public int X { get; set; } }

        [Fact]
        public void Example()
        {
            Mapper.CreateMap<Source, Target>();

            var source = new List<Source> { new Source { X = 42 } };
            var target = Mapper.Map<ReadOnlyCollection<Target>>(source);

            target.Count.ShouldEqual(source.Count);
            target[0].X.ShouldEqual(source[0].X);
        }
    }
}
