// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/OpenGenerics.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests
{
    using Should;
    using Assert = Xunit.Assert;

    public class OpenGenerics
    {
        public class Source<T>
        {
            public int A { get; set; }
            public T Value { get; set; }
        }

        public class Dest<T>
        {
            public int A { get; set; }
            public T Value { get; set; }
        }

        [Fact]
        public void Can_map_simple_generic_types()
        {
            Mapper.Initialize(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

            var source = new Source<int>
            {
                Value = 5
            };

            var dest = Mapper.Map<Source<int>, Dest<int>>(source);

            dest.Value.ShouldEqual(5);
        }

        [Fact]
        public void Can_map_non_generic_members()
        {
            Mapper.Initialize(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

            var source = new Source<int>
            {
                A = 5
            };

            var dest = Mapper.Map<Source<int>, Dest<int>>(source);

            dest.A.ShouldEqual(5);
        }

        [Fact]
        public void Can_map_recursive_generic_types()
        {
            Mapper.Initialize(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

            var source = new Source<Source<int>>
            {
                Value = new Source<int>
                {
                    Value = 5,
                }
            };

            var dest = Mapper.Map<Source<Source<int>>, Dest<Dest<double>>>(source);

            dest.Value.Value.ShouldEqual(5);
        }
    }
}
