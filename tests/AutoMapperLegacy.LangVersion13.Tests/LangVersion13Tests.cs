// A project on C# 13 (#14), written as code for AutoMapper 3.x was, with the README's global usings.
global using AutoMapperLegacy;
global using Mapper = AutoMapperLegacy.Mapper;
global using Profile = AutoMapperLegacy.Profile;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace LangVersion13App
{
    public class Order
    {
        public int Id { get; set; }
        public string Customer { get; set; }
        public List<string> Lines { get; set; } = new List<string>();
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string Customer { get; set; }
        public int LineCount { get; set; }
        public string Label { get; set; }
    }

    public class OrderSummary
    {
        public int Id { get; set; }
        public string Customer { get; set; }
    }

    public class LineCountResolver : ValueResolver<Order, int>
    {
        protected override int ResolveCore(Order source) => source.Lines.Count;
    }

    public class LabelConverter : TypeConverter<Order, string>
    {
        protected override string ConvertCore(Order source) => source.Customer + "#" + source.Id;
    }

    public class OrderProfile : Profile
    {
        protected override void Configure()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(d => d.LineCount, o => o.ResolveUsing<LineCountResolver>())
                .ForMember(d => d.Label, o => o.MapFrom(s => s.Customer + "#" + s.Id));
            CreateMap<Order, string>().ConvertUsing<LabelConverter>();
            CreateMap<Order, OrderSummary>();
        }
    }

    public class LangVersion13Tests : IDisposable
    {
        public LangVersion13Tests()
        {
            Mapper.Reset();
            Mapper.Initialize(c => c.AddProfile<OrderProfile>());
        }

        public void Dispose() => Mapper.Reset();

        [Fact]
        public void TheStaticApiWorksWithoutCSharp14()
        {
            var order = new Order { Id = 7, Customer = "Ada", Lines = { "tea", "jam" } };

            OrderDto dto = Mapper.Map<Order, OrderDto>(order);

            Assert.Equal(7, dto.Id);
            Assert.Equal(2, dto.LineCount);
            Assert.Equal("Ada#7", dto.Label);
            Assert.Equal("Ada#7", Mapper.Map<Order, string>(order));
        }

        [Fact]
        public void ProjectionWorksWithoutCSharp14()
        {
            var orders = new[] { new Order { Id = 1, Customer = "Ada" }, new Order { Id = 2, Customer = "Bo" } }.AsQueryable();

            Assert.Equal(new[] { "1 Ada", "2 Bo" }, orders.Project().To<OrderSummary>().Select(s => s.Id + " " + s.Customer).ToArray());
        }

        [Fact]
        public void ArrayContainsInAnExpressionTreeIsEnumerableContains()
        {
            // What LINQ to Entities, LINQ to SQL and an application's own expression visitors expect: C# 13 binds
            // array.Contains to Enumerable.Contains. C# 14 binds it to MemoryExtensions.Contains(ReadOnlySpan<T>, T)
            // through op_Implicit (AutoMapperLegacy.Tests: ExpressionTreeTests).
            int[] ids = { 1, 2 };
            Expression<Func<Order, bool>> query = o => ids.Contains(o.Id);

            var call = (MethodCallExpression)query.Body;
            Assert.Equal(typeof(Enumerable), call.Method.DeclaringType);
        }
    }
}
