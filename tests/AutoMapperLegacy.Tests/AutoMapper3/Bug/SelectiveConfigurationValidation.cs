// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/SelectiveConfigurationValidation.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Bug
{
    using System;
    using Assert = Xunit.Assert;

    public class SelectiveConfigurationValidation : NonValidatingSpecBase
    {
        public class GoodSrc { }
        public class GoodDest { }

        public class BadSrc
        {
            public Type BlowUp { get; set; }
        }

        public class BadDest
        {
            public int Value { get; set; }
            public int BlowUp { get; set; }
        }
        public class GoodProfile : Profile
        {
            protected override void Configure()
            {
                CreateMap<GoodSrc, GoodDest>();
            }
        }

        public class BadProfile : Profile
        {
            protected override void Configure()
            {
                CreateMap<BadSrc, BadDest>();
            }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<GoodProfile>();
                cfg.AddProfile<BadProfile>();
            });
        }

        [Fact]
        public void Should_pass_specific_profile_assertion()
        {
            typeof(AutoMapperConfigurationException)
                .ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid<GoodProfile>);
        }
    }
}
