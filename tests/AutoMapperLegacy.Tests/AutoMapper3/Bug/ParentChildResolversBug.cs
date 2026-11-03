// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/ParentChildResolversBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Bug
{
    namespace ParentChildResolversBug
    {
        using Should;
        using Assert = Xunit.Assert;

        public enum DestEnum
        {
            a,
            b,
            c,
        }

        public enum ParentDestEnum
        {
            d,
            e,
            f
        }

        public class ParentDest
        {
            public ParentDestEnum? field
            {
                get;
                set;
            }
        }

        public class Dest : ParentDest
        {
            public new DestEnum? field
            {
                get;
                set;
            }
        }

        public class Source
        {
            public string fieldCode
            {
                get;
                set;
            }
        }


        public class ParentResolver : ValueResolver<Source, ParentDestEnum?>
        {
            protected override ParentDestEnum? ResolveCore(Source source)
            {
                switch (source.fieldCode)
                {
                    case "testa": return ParentDestEnum.d;
                    case "testb": return ParentDestEnum.e;
                    case "testc": return ParentDestEnum.f;
                    default: return null;
                }
            }
        }

        public class Resolver : ValueResolver<Source, DestEnum?>
        {
            protected override DestEnum? ResolveCore(Source source)
            {
                switch (source.fieldCode)
                {
                    case "testa": return DestEnum.a;
                    case "testb": return DestEnum.b;
                    case "testc": return DestEnum.c;
                    default: return null;
                }
            }
        }

        public class ParentChildResolverTests : AutoMapperSpecBase
        {
            private Dest _dest;

            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.CreateMap<Source, ParentDest>()
                        .ForMember(dest => dest.field, opt => opt.ResolveUsing<ParentResolver>())
                        .Include<Source, Dest>();

                    cfg.CreateMap<Source, Dest>()
                        .ForMember(dest => dest.field, opt => opt.ResolveUsing<Resolver>());
                });
            }

            protected override void Because_of()
            {
                var source = new Source()
                {
                    fieldCode = "testa"
                };

                _dest = Mapper.Map<Dest>(source);
            }

            [Fact]
            public void Should_use_correct_resolver()
            {
                _dest.field.ShouldEqual(DestEnum.a);
            }
        }
    }
}
