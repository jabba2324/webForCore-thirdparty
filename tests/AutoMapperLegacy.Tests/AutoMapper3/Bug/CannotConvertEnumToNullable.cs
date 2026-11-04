// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/CannotConvertEnumToNullable.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class CannotConvertEnumToNullable
    {
        public enum DummyTypes : int
        {
            Foo = 1,
            Bar = 2
        }

        public class DummySource
        {
            public DummyTypes Dummy { get; set; }
        }

        public class DummyDestination
        {
            public int? Dummy { get; set; }
        }

        [Fact]
        public void Should_map_enum_to_nullable()
        {
            Mapper.CreateMap<DummySource, DummyDestination>();
            Mapper.AssertConfigurationIsValid();
            DummySource src = new DummySource() { Dummy = DummyTypes.Bar };

            var destination = Mapper.Map<DummySource, DummyDestination>(src);

            destination.Dummy.ShouldEqual((int)DummyTypes.Bar);
        }
    }
}
