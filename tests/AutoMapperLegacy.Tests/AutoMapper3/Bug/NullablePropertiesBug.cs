// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/NullablePropertiesBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Bug
{
    public class NullablePropertiesBug
    {
        public class Source { public int? A { get; set; } }
        public class Target { public int? A { get; set; } }

        [Fact]
        public void Example()
        {
            Mapper.CreateMap<Source, Target>();

            var d = Mapper.Map(new Source { A = null }, new Target { A = 10 });

            d.A.ShouldBeNull();
        }
    }
}
