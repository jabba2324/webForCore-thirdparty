// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/MappingInheritance/MultipleInheritedBaseMappingsOfSameTypeFails.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class MultipleMappingsOfSameTypeFails
    {
        public class MyClass
        {
            public ActivityBase Information { get; set; }
            public InformationClass CurrentInformation { get; set; }
        }

        public class MySpecificClass :MyClass{}

        public class MyDto
        {
            public InformationDto Information { get; set; }
        }

        public class MySpecificDto : MyDto{}
        public class InformationDto{}
        public class ActivityBase{}
        public class InformationBase{}
        public class InformationClass{}

        [Fact]
        public void multiple_inherited_base_mappings_of_same_type_fails()
        {
            Mapper.CreateMap<MyClass, MyDto>()
                .ForMember(d=> d.Information, m => m.MapFrom(s=>s.CurrentInformation))
                .Include<MySpecificClass, MySpecificDto>();
            Mapper.CreateMap<MySpecificClass, MySpecificDto>();

            Mapper.CreateMap<InformationClass, InformationDto>();

            Mapper.AssertConfigurationIsValid();
        }
    }
}
