// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Mappers/CustomMapperTests.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
#if AM3_ENGINE // Not compiled against the facade; why: Compat/Porting.cs (Skips)
using AutoMapper;
namespace AutoMapper3.UnitTests.Mappers
{
    namespace CustomMapperTests
    {
        // using AutoMapper.Mappers; (AutoMapper 3.x's namespace)
        using Assert = Xunit.Assert;

        public class When_adding_a_custom_mapper : NonValidatingSpecBase
        {
            protected override void Establish_context()
            {
                MapperRegistry.Mappers.Insert(0, new TestObjectMapper());

                Mapper.CreateMap<ClassA, ClassB>()
                    .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Source));
            }

            [Fact]
            public void Should_have_valid_configuration()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
            }


            public class TestObjectMapper : IObjectMapper
            {
                public object Map(ResolutionContext context, IMappingEngineRunner mapper)
                {
                    return new DestinationType();
                }

                public bool IsMatch(ResolutionContext context)
                {
                    return context.SourceType == typeof(SourceType) && context.DestinationType == typeof(DestinationType);
                }
            }

            public class ClassA
            {
                public SourceType Source { get; set; }
            }

            public class ClassB
            {
                public DestinationType Destination { get; set; }
            }

            public class SourceType
            {
                public int Value { get; set; }
            }

            public class DestinationType
            {
                public bool Value { get; set; }
            }
        }

    }
}
#endif
