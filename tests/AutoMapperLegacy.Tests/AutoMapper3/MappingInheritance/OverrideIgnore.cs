// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/MappingInheritance/OverrideIgnore.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class OverrideIgnore
    {
        public class DomainBase
        {
            public string SomeProperty { get; set; }
        }

        public class DtoBase
        {
            public string SomeDifferentProperty { get; set; }
        }

        [Fact]
        public void specifying_map_should_override_ignore()
        {
            Mapper.CreateMap<DomainBase, DtoBase>()
                .ForMember(m=>m.SomeDifferentProperty, m=>m.Ignore())
                .ForMember(m=>m.SomeDifferentProperty, m=>m.MapFrom(s=>s.SomeProperty));

            var dto = Mapper.Map<DomainBase, DtoBase>(new DomainBase {SomeProperty = "Test"});

            "Test".ShouldEqual(dto.SomeDifferentProperty);
        }

        [Fact]
        public void specifying_map_should_override_ignore_with_one_parameter()
        {
            Mapper.CreateMap<DomainBase, DtoBase>()
                .ForMember(m => m.SomeDifferentProperty, m => m.Ignore())
                .ForMember(m => m.SomeDifferentProperty, m => m.MapFrom(s => s.SomeProperty));

            var dto = Mapper.Map<DtoBase>(new DomainBase { SomeProperty = "Test" });

            "Test".ShouldEqual(dto.SomeDifferentProperty);
        }
    }
}
