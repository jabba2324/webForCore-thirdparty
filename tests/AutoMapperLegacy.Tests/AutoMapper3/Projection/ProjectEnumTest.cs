// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/ProjectEnumTest.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Projection
{
    using AutoMapper.QueryableExtensions;
    using Should;
    using System.Linq;
    using Assert = Xunit.Assert;

    public class ProjectEnumTest
    {
        public ProjectEnumTest()
        {
            Mapper.CreateMap<Customer, CustomerDto>();
            Mapper.CreateMap<CustomerType, string>().ProjectUsing(ct => ct.ToString().ToUpper());
        }

        [Fact]
        public void ProjectingEnumToString()
        {
            var customers = new[] { new Customer() { FirstName = "Bill", LastName = "White", CustomerType = CustomerType.Vip } }.AsQueryable();

            var projected = customers.Project().To<CustomerDto>();
            projected.ShouldNotBeNull();
            Assert.Equal(customers.Single().CustomerType.ToString().ToUpper(), projected.Single().CustomerType);
        }

        public class Customer
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public CustomerType CustomerType { get; set; }
        }

        public class CustomerDto
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string CustomerType { get; set; }
        }

        public enum CustomerType
        {
            Regular,
            Vip,
        }
    }

    public class ProjectionOverrides : NonValidatingSpecBase
    {
        public class Source
        {
            
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>()
                .ProjectUsing(src => new Dest {Value = 10});
        }

        [Fact]
        public void Should_validate_because_of_overridden_projection()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
        }
    }
}
