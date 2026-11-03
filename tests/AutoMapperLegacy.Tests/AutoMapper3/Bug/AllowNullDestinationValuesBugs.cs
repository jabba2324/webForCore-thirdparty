// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/AllowNullDestinationValuesBugs.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Bug
{
    namespace AllowNullDestinationValuesBugs
    {
        public class When_mapping_to_an_assignable_object_with_nullable_off : AutoMapperSpecBase
        {
            private Destination _destination;

            public class Source
            {
                public Inner Property { get; set; }
            }

            public class Destination
            {
                public Inner SomeOtherProperty { get; set; }
            }

            public class Inner
            {
                public string Name { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(config =>
                {
                    config.AllowNullDestinationValues = false;
                    config.CreateMap<Inner, Inner>();
                    config.CreateMap<Source, Destination>()
                        .ForMember(dest => dest.SomeOtherProperty, opt => opt.MapFrom(src => src.Property));
                });
            }

            protected override void Because_of()
            {
                Source source = new Source();

                _destination = Mapper.Map<Source, Destination>(source);
            }

            [Fact]
            public void Should_create_the_assingable_member()
            {
                _destination.ShouldNotBeNull();
                _destination.SomeOtherProperty.ShouldNotBeNull();
            }
        }
    }
}
