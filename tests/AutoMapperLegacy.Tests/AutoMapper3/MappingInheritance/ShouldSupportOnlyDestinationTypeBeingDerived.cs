// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/MappingInheritance/ShouldSupportOnlyDestinationTypeBeingDerived.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using AutoMapper.Mappers; (AutoMapper 3.x's namespace)
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.MappingInheritance
{
    public class DestinationTypePolymorphismTest
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class CustomerStubDTO
        {
            public int Id { get; set; }
        }

        public class CustomerDTO : CustomerStubDTO
        {
            public string Name { get; set; }
        }

        public class Order
        {
            public Customer Customer { get; set; }
        }

        public class OrderDTO
        {
            public CustomerStubDTO Customer { get; set; }
        }


        [Fact]
        public void Mapper_Should_Allow_Overriding_Of_Destination_Type()
        {
            var order = new Order() { Customer = new Customer() { Id = 1, Name = "A" } };
            Mapper.CreateMap<Order, OrderDTO>();
            Mapper.CreateMap<Customer, CustomerDTO>();
            Mapper.CreateMap<Customer, CustomerStubDTO>().As<CustomerDTO>();
            var orderDto = Mapper.Map<Order, OrderDTO>(order);

            var customerDto = (CustomerDTO)orderDto.Customer;
            "A".ShouldEqual(customerDto.Name);
            1.ShouldEqual(customerDto.Id);

        }

    }
    public class DestinationTypePolymorphismTestNonGeneric
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class CustomerStubDTO
        {
            public int Id { get; set; }
        }

        public class CustomerDTO : CustomerStubDTO
        {
            public string Name { get; set; }
        }

        public class Order
        {
            public Customer Customer { get; set; }
        }

        public class OrderDTO
        {
            public CustomerStubDTO Customer { get; set; }
        }


        [Fact]
        public void Mapper_Should_Allow_Overriding_Of_Destination_Type()
        {
            var order = new Order() { Customer = new Customer() { Id = 1, Name = "A" } };
            Mapper.CreateMap(typeof(Order), typeof(OrderDTO));
            Mapper.CreateMap(typeof(Customer), typeof(CustomerDTO));
            Mapper.CreateMap(typeof(Customer), typeof(CustomerStubDTO)).As(typeof(CustomerDTO));
            var orderDto = Mapper.Map<Order, OrderDTO>(order);

            var customerDto = (CustomerDTO)orderDto.Customer;
            "A".ShouldEqual(customerDto.Name);
            1.ShouldEqual(customerDto.Id);

        }

    }
}
