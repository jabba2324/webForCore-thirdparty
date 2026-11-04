// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Tests/MapperTests.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Tests
{
	public class MapperTests : NonValidatingSpecBase
	{
		public class Source
		{
			
		}
		
		public class Destination
		{
			
		}
			
		[Fact]
		public void Should_find_configured_type_map_when_two_types_are_configured()
		{
			Mapper.CreateMap<Source, Destination>();

			Mapper.FindTypeMapFor<Source, Destination>().ShouldNotBeNull();
		}
	}
}
