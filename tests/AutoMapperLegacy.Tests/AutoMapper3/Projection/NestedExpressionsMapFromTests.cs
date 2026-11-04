// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/NestedExpressionsMapFromTests.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Projection
{
    namespace NestedExpressionTests
    {
        using System.Collections.Generic;
        using System.Linq;
        using AutoMapper.QueryableExtensions;
#if RHINO_MOCKS
        using Rhino.Mocks.Constraints;
#endif
        using Should;
        using Assert = Xunit.Assert;

        public class NestedExpressionMapFromTests
        {
            public NestedExpressionMapFromTests()
            {
                Mapper.Initialize(cfg => 
                    cfg.CreateMap<Parent, ParentDto>()
                    .ForMember(dest => dest.TotalSum, opt => opt.MapFrom(p => p.Children.Sum(child => child.Value))));
            }

            [Fact]
            public void Should_use_nested_expression()
            {
                var items = new[]
                {
                    new Parent
                    {
                        Children = new List<Child>()
                        {
                            new Child() { Value = 4 },
                            new Child() { Value = 5 },
                        }
                    }
                };

                var projected = items.AsQueryable().Project().To<ParentDto>().ToList();

                projected[0].TotalSum.ShouldEqual(9);
            }
        }

        public class ParentDto
        {
            public int TotalSum { get; set; }
        }


        public class Parent
        {
            public List<Child> Children { get; set; }
        }

        public class Child
        {
            public int Value { get; set; }
        }
    }
}
