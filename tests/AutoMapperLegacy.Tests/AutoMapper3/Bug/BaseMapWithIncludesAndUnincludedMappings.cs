// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/BaseMapWithIncludesAndUnincludedMappings.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class BaseMapWithIncludesAndUnincludedMappings
    {
        public class ADTO
        {
            public int Prop { get; set; }
        }

        public class BDTO : ADTO
        {
            public int PropB { get; set; }
        }

        public class BDTO2 : ADTO
        {

        }

        public class A
        {
            public int Prop { get; set; }
        }

        public class B : A
        {
            public int PropB { get; set; }
        }

        [PortSkip(Skips.Behaviour)]
        [Fact]
        public void base_has_include_of_source_but_mapping_with_both_sides_being_unmapped_types_from_the_base_fails()
        {
            Mapper.CreateMap<A, ADTO>().Include<B, BDTO>();
            Mapper.CreateMap<B, BDTO>();
            var a = Mapper.Map<A, ADTO>(new B(), new BDTO2()); // Throws invalid cast exception trying to convert BDTO2 to BDTO
        }
    }
}
