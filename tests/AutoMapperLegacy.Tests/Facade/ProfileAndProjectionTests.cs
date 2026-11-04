// Written as code for AutoMapper 3.x was: `using AutoMapper;` and the static Mapper (GlobalUsings.cs has the aliases).
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Xunit;

namespace LegacyApp
{
    // A 3.x profile that calls the static Mapper in Configure, as SocialGoal's do.
    public class DomainToViewModelProfile : Profile
    {
        public override string ProfileName => "DomainToViewModel";

        protected override void Configure()
        {
            Mapper.CreateMap<OrderLine, OrderLineDto>();
        }
    }

    // A 3.x profile that creates its maps itself, with its own settings.
    public class PrefixProfile : Profile
    {
        protected override void Configure()
        {
            RecognizePrefixes("Customer");
            CreateMap<OrderDto, Customer>().ForMember(d => d.Email, o => o.Ignore());
        }
    }

    // 4.x-style: maps created in the constructor.
    public class ConstructorProfile : Profile
    {
        public ConstructorProfile()
        {
            CreateMap<Customer, OrderLineDto>().ForMember(d => d.Product, o => o.MapFrom(s => s.Name)).ForMember(d => d.Quantity, o => o.Ignore());
        }
    }

    // An AutoMapper 12 profile, as code that already moved to the instance API has.
    public class NativeProfile : AutoMapper.Profile
    {
        public NativeProfile()
        {
            CreateMap<OrderLineDto, OrderLine>().ForMember(d => d.Price, o => o.Ignore());
        }
    }

    public class ProfileTests : StaticMapperTest
    {
        [Fact]
        public void Initialize_with_AddProfile()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DomainToViewModelProfile>();
                x.AddProfile<PrefixProfile>();
                x.AddProfile(new ConstructorProfile());
                x.AddProfile<NativeProfile>();
            });

            Assert.Equal("Tea", Mapper.Map<OrderLineDto>(new OrderLine { Product = "Tea" }).Product);
            Assert.Equal("Ada", Mapper.Map<Customer>(new OrderDto { CustomerName = "Ada" }).Name);
            Assert.Equal("Bob", Mapper.Map<OrderLineDto>(new Customer { Name = "Bob" }).Product);
            Assert.Equal(3, Mapper.Map<OrderLine>(new OrderLineDto { Quantity = 3 }).Quantity);
            Mapper.AssertConfigurationIsValid();
            Mapper.AssertConfigurationIsValid<PrefixProfile>();
        }

        [Fact]
        public void Configure_runs_once()
        {
            var profile = new CountingProfile();
            Mapper.AddProfile(profile);
            Mapper.Map<OrderLineDto>(new OrderLine());
            Mapper.CreateMap<Order, OrderDto>();
            Mapper.Map<OrderLineDto>(new OrderLine());

            Assert.Equal(1, profile.Configured);
        }

        [Fact]
        public void CreateProfile_by_name()
        {
            Mapper.CreateProfile("Named", p =>
            {
                p.AllowNullCollections = true;
                p.CreateMap<Order, OrderDto>();
            });
            Mapper.CreateMap<OrderLine, OrderLineDto>();

            Assert.Null(Mapper.Map<OrderDto>(new Order { Lines = null }).Lines);
            Mapper.AssertConfigurationIsValid("Named");
        }

        private class CountingProfile : Profile
        {
            public int Configured;

            protected override void Configure()
            {
                Configured++;
                CreateMap<OrderLine, OrderLineDto>();
            }
        }
    }

    public class ProjectionTests : StaticMapperTest
    {
        private static IQueryable<OrderLine> Lines() => new List<OrderLine>
        {
            new OrderLine { Product = "Tea", Quantity = 2 },
            new OrderLine { Product = "Jam", Quantity = 1 },
        }.AsQueryable();

        [Fact]
        public void Project_To()
        {
            Mapper.CreateMap<OrderLine, OrderLineDto>();

            var dtos = Lines().Project().To<OrderLineDto>().ToList();

            Assert.Equal(new[] { "Tea", "Jam" }, dtos.Select(d => d.Product));
        }

        [Fact]
        public void ProjectTo_without_a_configuration()
        {
            Mapper.CreateMap<OrderLine, OrderLineDto>().ForMember(d => d.Product, o => o.MapFrom(s => s.Product.ToUpper()));

            Assert.Equal("TEA", Lines().ProjectTo<OrderLineDto>().First().Product);
        }

        [Fact]
        public void Project_with_the_engine_and_ProjectUsing()
        {
            Mapper.CreateMap<OrderLine, string>().ProjectUsing(l => l.Product + l.Quantity);

            Assert.Equal(new[] { "Tea2", "Jam1" }, Lines().Project(Mapper.Engine).To<string>().ToArray());
        }
    }
}
