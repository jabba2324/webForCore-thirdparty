// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/NullableItemsTests.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System;

namespace AutoMapper3.UnitTests.Projection
{
    namespace NullableItemsTests
    {
        using System.Linq;
        using AutoMapper.QueryableExtensions;
        using Should;
        using Assert = Xunit.Assert;

        [PortSkip(Skips.Projection)]
        public class NullChildItemTest
        {
            public NullChildItemTest()
            {
                Mapper.Initialize(cfg => cfg.CreateMap<Parent, ParentDto>());
            }

            [Fact]
            public void Should_project_null_value()
            {
                var items = new[]
                {
                    new Parent
                    {
                        Value = 5
                    }
                };

                var projected = items.AsQueryable().Project().To<ParentDto>().ToList();

                projected[0].Value.ShouldEqual(5);
                projected[0].ChildValue.ShouldBeNull();
                projected[0].ChildGrandChildValue.ShouldBeNull();
            }


            public class ParentDto
            {
                public int? Value { get; set; }
                public int? ChildValue { get; set; }
                public int? ChildGrandChildValue { get; set; }
                public DateTime? Date { get; set; }
            }


            public class Parent
            {
                public int Value { get; set; }
                public Child Child { get; set; }
            }

            public class Child
            {
                public int Value { get; set; }
                public GrandChild GrandChild { get; set; }
            }

            public class GrandChild
            {
                public int Value { get; set; }
            }
        }

        public class CustomMapFromTest
        {
            public class Parent
            {
                public int Value { get; set; }
                
            }

            public class ParentDto
            {
                public int? Value { get; set; }
                public DateTime? Date { get; set; }
            }
            public CustomMapFromTest()
            {
                Mapper.Initialize(cfg => cfg.CreateMap<Parent, ParentDto>()
                    .ForMember(dto => dto.Date, opt => opt.MapFrom(src => DateTime.UtcNow)));
            }

            [Fact]
            public void Should_not_fail()
            {
                var items = new[]
                {
                    new Parent
                    {
                        Value = 5
                    }
                };

                var projected = items.AsQueryable().Project().To<ParentDto>().ToList();

                typeof(NullReferenceException).ShouldNotBeThrownBy(() => items.AsQueryable().Project().To<ParentDto>().ToList());
                Assert.NotNull(projected[0].Date);
            }
        }
    }
}
