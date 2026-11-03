// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/ProjectCollectionsBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Bug
{
    using System.Collections.Generic;

    namespace ProjectCollectionsBug
    {
        using System;
        using System.Linq;
        using AutoMapper.QueryableExtensions;
        using Assert = Xunit.Assert;

        public class A
        {
            public int AP1 { get; set; }
            public string AP2 { get; set; }
        }

        public class B
        {
            public B()
            {
                BP2 = new HashSet<A>();
            }
            public int BP1 { get; set; }
            public ICollection<A> BP2 { get; set; }
        }

        public class AEntity
        {
            public int AP1 { get; set; }
            public string AP2 { get; set; }
        }

        public class BEntity
        {
            public BEntity()
            {
                BP2 = new HashSet<AEntity>();
            }
            public int BP1 { get; set; }
            public ICollection<AEntity> BP2 { get; set; }
        }

        public class Bug
        {
            [Fact]
            public void Should_not_throw_exception()
            {
                Mapper.CreateMap<BEntity, B>();
                Mapper.CreateMap<AEntity, A>();
                Mapper.AssertConfigurationIsValid();

                var be = new BEntity();
                be.BP1 = 3;
                be.BP2.Add(new AEntity() { AP1 = 1, AP2 = "hello" });
                be.BP2.Add(new AEntity() { AP1 = 2, AP2 = "two" });

                var b = Mapper.Map<BEntity, B>(be);

                var belist = new List<BEntity>();
                belist.Add(be);
                IQueryable<BEntity> bei = belist.AsQueryable();
                typeof(Exception).ShouldNotBeThrownBy(() => bei.Project().To<B>());
            }
        }
    }
}
