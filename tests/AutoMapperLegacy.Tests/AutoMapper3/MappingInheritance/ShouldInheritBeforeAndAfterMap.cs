// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/MappingInheritance/ShouldInheritBeforeAndAfterMap.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
#if AM3_ENGINE // Not compiled against the facade; why: Compat/Porting.cs (Skips)
// using AutoMapper.Mappers; (AutoMapper 3.x's namespace)
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.MappingInheritance
{
    public class ShouldInheritBeforeAndAfterMap
    {
        public class BaseClass
        {
            public string Prop { get; set; }
        } 
        public class Class : BaseClass {}

        public class BaseDto
        {
            public string DifferentProp { get; set; }            
        }
        public class Dto : BaseDto {}

        [Fact]
        public void should_inherit_base_beforemap()
        {
            // arrange
            var source = new Class{ Prop = "test" };
            var configurationProvider = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            configurationProvider
                .CreateMap<BaseClass, BaseDto>()
                .BeforeMap((s, d) => d.DifferentProp = s.Prop)
                .Include<Class, Dto>();

            configurationProvider.CreateMap<Class, Dto>();
            var mappingEngine = new MappingEngine(configurationProvider);

            // act
            var dest = mappingEngine.Map<Class, Dto>(source);

            // assert
            "test".ShouldEqual(dest.DifferentProp);
        }

        [Fact]
        public void should_inherit_base_aftermap()
        {
            // arrange
            var source = new Class { Prop = "test" };
            var configurationProvider = new ConfigurationStore(new TypeMapFactory(), MapperRegistry.Mappers);
            configurationProvider
                .CreateMap<BaseClass, BaseDto>()
                .AfterMap((s, d) => d.DifferentProp = s.Prop)
                .Include<Class, Dto>();

            configurationProvider.CreateMap<Class, Dto>();
            var mappingEngine = new MappingEngine(configurationProvider);

            // act
            var dest = mappingEngine.Map<Class, Dto>(source);

            // assert
            "test".ShouldEqual(dest.DifferentProp);
        }
    }
}
#endif
