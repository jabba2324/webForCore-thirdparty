// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/InheritedMaps.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Projection
{
    namespace InheritedMaps
    {
        using System.Linq;
        using AutoMapper.QueryableExtensions;
        using Should;
        using Assert = Xunit.Assert;

        public class SourceBase
        {
            public int OtherValue { get; set; }
        }

        public class Source : SourceBase
        {
            
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        public class When_projecting_and_inheriting_maps : AutoMapperSpecBase
        {
            private Dest[] _dest;

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<SourceBase, Dest>()
                        .Include<Source, Dest>()
                        .ForMember(d => d.Value, opt => opt.MapFrom(src => src.OtherValue));

                    cfg.CreateMap<Source, Dest>();
                });
            }

            protected override void Because_of()
            {
                IQueryable<Source> sources = new[]
                {
                    new Source()
                    {
                        OtherValue = 10
                    }
                }.AsQueryable();

                _dest = sources.Project().To<Dest>().ToArray();
            }

            [Fact]
            public void Should_inherit_base_mapping()
            {
                _dest[0].Value.ShouldEqual(10);
            }
        }
    }
}
