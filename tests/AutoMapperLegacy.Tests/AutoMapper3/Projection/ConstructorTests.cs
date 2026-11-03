// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/ConstructorTests.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Projection
{
    using System.Linq;
    using AutoMapper.QueryableExtensions;
    using Should;
    using Assert = Xunit.Assert;

    public class ConstructorTests : AutoMapperSpecBase
    {
        private Dest[] _dest;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public Dest()
            {
                
            }
            public Dest(int other)
            {
                Other = other;
            }

            public int Value { get; set; }
            [IgnoreMap]
            public int Other { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>()
                .ConstructProjectionUsing(src => new Dest(src.Value + 10));
        }

        protected override void Because_of()
        {
            var values = new[]
            {
                new Source()
                {
                    Value = 5
                }
            }.AsQueryable();

            _dest = values.Project().To<Dest>().ToArray();
        }

        [Fact]
        public void Should_construct_correctly()
        {
            _dest[0].Other.ShouldEqual(15);
        }
    }
}
