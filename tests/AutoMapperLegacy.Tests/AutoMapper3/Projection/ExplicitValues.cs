// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/ExplicitValues.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Projection
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper.QueryableExtensions;
    using Should;
    using Assert = Xunit.Assert;

    [PortSkip(Skips.Projection)]
    public class ExplicitValues : AutoMapperSpecBase
    {
        private List<Dest> _dests;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.UseValue(5));
        }

        protected override void Because_of()
        {
            var source = new[] { new Source { Value = 10 } }.AsQueryable();

            _dests = source.Project().To<Dest>().ToList();
        }

        [Fact]
        public void Should_substitute_value()
        {
            _dests[0].Value.ShouldEqual(5);
        }
    }

    public class ExplicitValues_ForReset : AutoMapperSpecBase
    {
        private List<Dest> _dests;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.UseValue(5));

            new[] { new Source { Value = 10 } }.AsQueryable().Project().To<Dest>().ToList();

            Mapper.Reset();

            Mapper.CreateMap<Source, Dest>()
                    .ForMember(dest => dest.Value, opt => opt.UseValue(10));
        }

        protected override void Because_of()
        {
            var source = new[] { new Source { Value = 10 } }.AsQueryable();

            _dests = source.Project().To<Dest>().ToList();
        }

        [Fact]
        public void Should_substitute_value()
        {
            _dests[0].Value.ShouldEqual(10);
        }
    }
}
