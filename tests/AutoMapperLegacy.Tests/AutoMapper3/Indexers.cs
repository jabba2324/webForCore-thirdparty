// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Indexers.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests
{
	namespace Indexers
	{
		public class When_mapping_to_a_destination_with_an_indexer_property : AutoMapperSpecBase
		{
			private Destination _result;

			public class Source
			{
				public string Value { get; set; }
			}

			public class Destination
			{
				public string Value { get; set; }
				public string this[string key] { get { return null; }}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Source, Destination>(new Source {Value = "Bob"});
			}

			[Fact]
			public void Should_ignore_indexers_and_map_successfully()
			{
				_result.Value.ShouldEqual("Bob");
			}

			[Fact]
			public void Should_pass_configuration_check()
			{
				Exception thrown = null;
				try
				{
					Mapper.AssertConfigurationIsValid();
				}
				catch (Exception ex)
				{
					thrown = ex;
				}

				thrown.ShouldBeNull();
			}
		}

	}
}
