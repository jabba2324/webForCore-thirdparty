// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/NullSubstitutes.cs
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

    public class NullSubstitutes : AutoMapperSpecBase
    {
        private List<Dest> _dests;

        public class Source
        {
            public int? Value { get; set; }
        }

        public class Dest
        {
            public int? Value { get; set; }            
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>().ForMember(m => m.Value, opt => opt.NullSubstitute(5));
        }

        protected override void Because_of()
        {
            var source = new[] {new Source()}.AsQueryable();

            _dests = source.Project().To<Dest>().ToList();
        }

        [Fact]
        public void Can_substitute_null_values()
        {
            _dests[0].Value.ShouldEqual(5);
        }
    }

    public class NullSubstitutesWithMapFrom : AutoMapperSpecBase
    {
        private List<Dest> _dests;

        public class Source
        {
            public int? Value { get; set; }
        }

        public class Dest
        {
            public int? ValuePropertyNotMatching { get; set; }            
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>().ForMember(m => m.ValuePropertyNotMatching, opt =>
            {
                opt.MapFrom(src => src.Value);
                opt.NullSubstitute(5);
            });
        }

        protected override void Because_of()
        {
            var source = new[] {new Source()}.AsQueryable();

            _dests = source.Project().To<Dest>().ToList();
        }

        [Fact]
        public void Can_substitute_null_values()
        {
            _dests[0].ValuePropertyNotMatching.ShouldEqual(5);
        }
    }
}
