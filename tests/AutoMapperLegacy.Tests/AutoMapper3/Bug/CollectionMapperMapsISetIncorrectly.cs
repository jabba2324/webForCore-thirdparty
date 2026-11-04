// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/CollectionMapperMapsISetIncorrectly.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
#if AM3_ENGINE // Not compiled against the facade; why: Compat/Porting.cs (Skips)
#if !WINDOWS_PHONE
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
// using AutoMapper.Mappers; (AutoMapper 3.x's namespace)
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class CollectionMapperMapsIEnumerableToISetIncorrectly
    {
        public class TypeWithStringProperty
        {
            public string Value { get; set; }
        }

        public class SourceWithIEnumerable
        {
            public IEnumerable<TypeWithStringProperty> Stuff { get; set; }
        }

        public class TargetWithISet
        {
            public ISet<string> Stuff { get; set; }
        }

        [Fact]
        public void ShouldMapToNewISet()
        {
            new PlatformSpecificMapperRegistryOverride().Initialize();
            var config = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            config.CreateMap<SourceWithIEnumerable, TargetWithISet>()
                  .ForMember(dest => dest.Stuff, opt => opt.MapFrom(src => src.Stuff.Select(s => s.Value)));

            config.AssertConfigurationIsValid();

            var engine = new MappingEngine(config);

            var source = new SourceWithIEnumerable
            {
                Stuff = new[]
                            {
                                new TypeWithStringProperty { Value = "Microphone" }, 
                                new TypeWithStringProperty { Value = "Check" }, 
                                new TypeWithStringProperty { Value = "1, 2" }, 
                                new TypeWithStringProperty { Value = "What is this?" }
                            }
            };

            var target = engine.Map<SourceWithIEnumerable, TargetWithISet>(source);
        }
    }
}
#endif
#endif
