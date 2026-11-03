// Written as code for AutoMapper 3.x was: `using AutoMapper;` and the static Mapper (GlobalUsings.cs has the aliases).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Xunit;

// Not in AutoMapperLegacy's namespace, so names resolve as they do in an application.
namespace LegacyApp
{
    public class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; }
        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
        public decimal GetTotal() => Lines?.Sum(l => l.Price * l.Quantity) ?? 0;
    }

    public class Customer
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class OrderLine
    {
        public string Product { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
        public List<OrderLineDto> Lines { get; set; }
    }

    public class OrderLineDto
    {
        public string Product { get; set; }
        public int Quantity { get; set; }
    }

    public abstract class StaticMapperTest : IDisposable
    {
        protected StaticMapperTest() => Mapper.Reset();

        public void Dispose() => Mapper.Reset();
    }

    public class StaticApiTests : StaticMapperTest
    {
        private static Order NewOrder() => new Order
        {
            Id = 7,
            Customer = new Customer { Name = "Ada", Email = "ada@example.org" },
            Lines = { new OrderLine { Product = "Tea", Price = 2.5m, Quantity = 2 } },
        };

        [Fact]
        public void CreateMap_then_Map_flattens_and_maps_collections()
        {
            Mapper.CreateMap<Order, OrderDto>();
            Mapper.CreateMap<OrderLine, OrderLineDto>();

            var dto = Mapper.Map<Order, OrderDto>(NewOrder());

            Assert.Equal(7, dto.Id);
            Assert.Equal("Ada", dto.CustomerName);
            Assert.Equal(5m, dto.Total);
            Assert.Equal("Tea", Assert.Single(dto.Lines).Product);
        }

        [Fact]
        public void Maps_added_after_the_first_Map_are_used()
        {
            Mapper.CreateMap<OrderLine, OrderLineDto>();
            Assert.Equal("Tea", Mapper.Map<OrderLineDto>(NewOrder().Lines[0]).Product);

            Mapper.CreateMap<Order, OrderDto>();

            Assert.Equal("Ada", Mapper.Map<OrderDto>(NewOrder()).CustomerName);
        }

        [Fact]
        public void A_map_can_be_configured_after_the_first_Map()
        {
            var map = Mapper.CreateMap<OrderLine, OrderLineDto>();
            Assert.Equal("Tea", Mapper.Map<OrderLineDto>(NewOrder().Lines[0]).Product);

            map.ForMember(d => d.Product, o => o.Ignore());

            Assert.Null(Mapper.Map<OrderLineDto>(NewOrder().Lines[0]).Product);
        }

        [Fact]
        public void CreateMap_twice_adds_to_the_same_map()
        {
            Mapper.CreateMap<OrderLine, OrderLineDto>().ForMember(d => d.Product, o => o.MapFrom(s => s.Product.ToUpper()));
            Mapper.CreateMap<OrderLine, OrderLineDto>().ForMember(d => d.Quantity, o => o.Ignore());

            var dto = Mapper.Map<OrderLineDto>(NewOrder().Lines[0]);

            Assert.Equal("TEA", dto.Product);
            Assert.Equal(0, dto.Quantity);
        }

        [Fact]
        public void Initialize_replaces_the_configuration()
        {
            Mapper.CreateMap<Order, OrderDto>();
            Mapper.Initialize(cfg => cfg.CreateMap<OrderLine, OrderLineDto>());

            Assert.Null(Mapper.FindTypeMapFor<Order, OrderDto>());
            Assert.NotNull(Mapper.FindTypeMapFor<OrderLine, OrderLineDto>());
        }

        [Fact]
        public void Map_overloads()
        {
            Mapper.CreateMap<OrderLine, OrderLineDto>();
            var line = NewOrder().Lines[0];

            Assert.Equal("Tea", ((OrderLineDto)Mapper.Map(line, typeof(OrderLine), typeof(OrderLineDto))).Product);
            var existing = new OrderLineDto();
            Assert.Same(existing, Mapper.Map(line, existing));
            Assert.Equal(2, existing.Quantity);
            var other = new OrderLineDto();
            Mapper.Map(line, other, typeof(OrderLine), typeof(OrderLineDto));
            Assert.Equal("Tea", other.Product);
            Assert.Equal("Tea", Mapper.Map<OrderLine, OrderLineDto>(line, opts => opts.Items["x"] = 1).Product);
            Assert.Equal(2, Mapper.Map<List<OrderLineDto>>(new List<OrderLine> { line, line }).Count);
            Assert.Equal("Tea", Mapper.Engine.Map<OrderLineDto>(line).Product);
            Assert.Equal("Tea", Mapper.Instance.Map<OrderLineDto>(line).Product);
        }

        [Fact]
        public void Mapping_without_a_map_throws()
        {
            Assert.Throws<AutoMapperMappingException>(() => Mapper.Map<OrderLineDto>(new OrderLine()));
        }

        [Fact]
        public void DynamicMap_creates_the_map()
        {
            var dto = Mapper.DynamicMap<OrderLine, OrderLineDto>(NewOrder().Lines[0]);

            Assert.Equal("Tea", dto.Product);
            Assert.NotNull(Mapper.FindTypeMapFor<OrderLine, OrderLineDto>());
            Assert.Equal(2, Mapper.DynamicMap<OrderLineDto>(NewOrder().Lines[0]).Quantity);
        }

        [Fact]
        public void CreateMissingTypeMaps_creates_the_top_level_map()
        {
            Mapper.Initialize(cfg => cfg.CreateMissingTypeMaps = true);

            Assert.Equal("Tea", Mapper.Map<OrderLineDto>(NewOrder().Lines[0]).Product);
        }

        [Fact]
        public void AssertConfigurationIsValid_reports_unmapped_members()
        {
            Mapper.CreateMap<OrderLine, Customer>();

            Assert.Throws<AutoMapperConfigurationException>(() => Mapper.AssertConfigurationIsValid());
        }

        [Fact]
        public void AssertConfigurationIsValid_passes_with_ignored_members()
        {
            Mapper.CreateMap<OrderLine, Customer>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Product))
                .ForMember(d => d.Email, o => o.Ignore());

            Mapper.AssertConfigurationIsValid();
            Assert.Single(Mapper.GetAllTypeMaps());
        }

        [Fact]
        public void ReverseMap()
        {
            Mapper.CreateMap<OrderLine, OrderLineDto>().ReverseMap().ForMember(d => d.Price, o => o.UseValue(1m));

            var line = Mapper.Map<OrderLine>(new OrderLineDto { Product = "Tea", Quantity = 3 });

            Assert.Equal(3, line.Quantity);
            Assert.Equal(1m, line.Price);
        }

        [Fact]
        public void ConvertUsing_and_ConstructUsing_take_statement_lambdas()
        {
            Mapper.CreateMap<OrderLine, string>().ConvertUsing(line =>
            {
                var text = line.Product + " x" + line.Quantity;
                return text;
            });
            Mapper.CreateMap<OrderLineDto, OrderLine>().ConstructUsing(dto =>
            {
                var line = new OrderLine();
                line.Price = 9m + dto.Quantity;
                return line;
            });

            Assert.Equal("Tea x2", Mapper.Map<string>(NewOrder().Lines[0]));
            Assert.Equal(9m, Mapper.Map<OrderLine>(new OrderLineDto()).Price);
        }

        [Fact]
        public void BeforeMap_AfterMap_and_ForAllMembers()
        {
            Mapper.CreateMap<OrderLine, OrderLineDto>()
                .BeforeMap((s, d) => d.Quantity = 100)
                .AfterMap((s, d) => d.Product += "!")
                .ForAllMembers(o => o.Condition((ResolutionContext c) => !c.IsSourceValueNull));

            var dto = Mapper.Map<OrderLineDto>(new OrderLine { Product = null, Quantity = 1 });

            Assert.Equal("!", dto.Product);
            Assert.Equal(1, dto.Quantity);
        }

        [Fact]
        public void CreateMap_with_types()
        {
            Mapper.CreateMap(typeof(OrderLine), typeof(OrderLineDto)).ForMember("Product", o => o.UseValue("fixed"));

            var dto = (OrderLineDto)Mapper.Map(new OrderLine { Quantity = 4 }, typeof(OrderLine), typeof(OrderLineDto));

            Assert.Equal("fixed", dto.Product);
            Assert.Equal(4, dto.Quantity);
        }

        [Fact]
        public void Global_settings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AllowNullCollections = true;
                cfg.CreateMap<Order, OrderDto>();
                cfg.CreateMap<OrderLine, OrderLineDto>();
            });

            Assert.Null(Mapper.Map<OrderDto>(new Order { Lines = null }).Lines);
        }

        [Fact]
        public void Mapping_is_thread_safe_while_maps_are_added()
        {
            Mapper.CreateMap<Order, OrderDto>();
            Mapper.CreateMap<OrderLine, OrderLineDto>();

            Parallel.For(0, 200, i =>
            {
                if (i % 20 == 0)
                    Mapper.CreateMap<Customer, OrderLineDto>().ForMember(d => d.Product, o => o.MapFrom(s => s.Name));
                Assert.Equal("Ada", Mapper.Map<OrderDto>(NewOrder()).CustomerName);
            });
        }
    }
}
