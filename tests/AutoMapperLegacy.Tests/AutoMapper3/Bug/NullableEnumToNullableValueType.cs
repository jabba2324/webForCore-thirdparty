// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/NullableEnumToNullableValueType.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class NullableEnumToNullableValueType
    {
        public class CannotConvertEnumToNullableWhenPassedNull : AutoMapperSpecBase
        {
            public enum DummyTypes : int
            {
                Foo = 1,
                Bar = 2
            }

            public class DummySource
            {
                public DummyTypes? Dummy { get; set; }
            }

            public class DummyDestination
            {
                public int? Dummy { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.CreateMap<DummySource, DummyDestination>();
            }

            [Fact]
            public void Should_map_null_enum_to_nullable_base_type()
            {
                DummySource src = new DummySource() { Dummy = null };

                var destination = Mapper.Map<DummySource, DummyDestination>(src);

                destination.Dummy.ShouldBeNull();
            }
        } 
    }
}
