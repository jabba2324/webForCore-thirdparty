// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/ResolutionContextTester.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
#if AM3_ENGINE // Not compiled against the facade; why: Compat/Porting.cs (Skips)
using AutoMapper;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests
{
	public class ResolutionContextTester
	{
		public string DummyProp { get; set; }

		[Fact]
		public void When_creating_a_new_context_from_an_existing_context_Should_preserve_context_type_map()
		{
			var map = new TypeMap(new TypeInfo(typeof(int)), new TypeInfo(typeof(string)), MemberList.Destination);

			var context = new ResolutionContext(map, 5, typeof(int), typeof(string), new MappingOperationOptions(), Mapper.Engine);

			ResolutionContext newContext = context.CreateValueContext(10);

			newContext.GetContextTypeMap().ShouldNotBeNull();
		}
	}
}
#endif
