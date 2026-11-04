// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/MappingInheritance/IgnoreShouldBeInheritedIfConventionCannotMap.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class IgnoreShouldBeInheritedIfConventionCannotMap
    {
        public class BaseDomain
        {

        }

        public class StandardDomain : BaseDomain
        {
            
        }

        public class SpecificDomain : StandardDomain
        {
        }

        public class MoreSpecificDomain : SpecificDomain
        {
            
        }

        public class Dto
        {
            public string SpecificProperty { get; set; }
        }

        [Fact]
        public void inhertited_ignore_should_be_overridden_passes_validation()
        {
            Mapper.CreateMap<BaseDomain, Dto>()
                .ForMember(d => d.SpecificProperty, m => m.Ignore())
                .Include<StandardDomain, Dto>();

            Mapper.CreateMap<StandardDomain, Dto>()
                .Include<SpecificDomain, Dto>();

            Mapper.CreateMap<SpecificDomain, Dto>()
                .Include<MoreSpecificDomain, Dto>();

            Mapper.CreateMap<MoreSpecificDomain, Dto>();

            Mapper.AssertConfigurationIsValid();
        }
    }
}
