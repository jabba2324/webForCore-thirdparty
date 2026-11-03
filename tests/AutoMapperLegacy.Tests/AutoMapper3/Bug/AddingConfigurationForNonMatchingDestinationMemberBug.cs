// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/AddingConfigurationForNonMatchingDestinationMemberBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    namespace AddingConfigurationForNonMatchingDestinationMember
    {
        public class AddingConfigurationForNonMatchingDestinationMemberBug : NonValidatingSpecBase
        {
            public class Source
            {
                
            }

            public class Destination
            {
                public string Value { get; set; }
            }

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, Destination>()
                        .ForMember(dest => dest.Value, opt => opt.NullSubstitute("Foo"));
                });
            }

            [Fact]
            public void Should_show_configuration_error()
            {
                typeof (AutoMapperConfigurationException).ShouldBeThrownBy(Mapper.AssertConfigurationIsValid);
            }
        }
    }
}
