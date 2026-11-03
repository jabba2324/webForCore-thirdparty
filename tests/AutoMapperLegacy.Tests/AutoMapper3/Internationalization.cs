// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Internationalization.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests
{
	namespace Internationalization
	{
		public class When_mapping_a_source_with_non_english_property_names : AutoMapperSpecBase
		{
			private OrderDto _result;

			public class Order
			{
				public Customer Customer { get; set; }
			}

			public class Customer
			{
				public string Æøå { get; set; }
			}

			public class OrderDto
			{
				public string CustomerÆøå { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Order, OrderDto>();
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Order, OrderDto>(new Order {Customer = new Customer {Æøå = "Bob"}});
			}

			[Fact]
			public void Should_match_to_identical_property_name_on_destination()
			{
				_result.CustomerÆøå.ShouldEqual("Bob");
			}
		}

	}
}
