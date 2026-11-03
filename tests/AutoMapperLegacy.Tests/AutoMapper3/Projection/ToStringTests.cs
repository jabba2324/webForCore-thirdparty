// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/ToStringTests.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Projection
{
    using System.Linq;
    using AutoMapper.QueryableExtensions;
    using Should;
    using Assert = Xunit.Assert;

    public class ToStringTests : AutoMapperSpecBase
    {
        private Dest[] _dests;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public string Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>();
        }

        protected override void Because_of()
        {
            var sources = new[]
            {
                new Source
                {
                    Value = 5
                }
            }.AsQueryable();

            _dests = sources.Project().To<Dest>().ToArray();
        }

        [Fact]
        public void Should_convert_to_string()
        {
            _dests[0].Value.ShouldEqual("5");
        }
    }
}
