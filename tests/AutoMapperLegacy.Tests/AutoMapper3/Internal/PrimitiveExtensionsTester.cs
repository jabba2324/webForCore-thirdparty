// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Internal/PrimitiveExtensionsTester.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
#if AM3_ENGINE // Not compiled against the facade; why: Compat/Porting.cs (Skips)
using AutoMapper;
using System.Collections;
using System.Collections.Generic;
using AutoMapper.Internal;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests
{
	public class PrimitiveExtensionsTester
	{
		[Fact]
		public void Should_not_flag_only_enumerable_type_as_writeable_collection()
		{
			typeof(string).IsListOrDictionaryType().ShouldBeFalse();
		}

		[Fact]
		public void Should_flag_list_as_writable_collection()
		{
			typeof(int[]).IsListOrDictionaryType().ShouldBeTrue();
		}

		[Fact]
		public void Should_flag_generic_list_as_writeable_collection()
		{
			typeof(List<int>).IsListOrDictionaryType().ShouldBeTrue();
		}

		[Fact]
		public void Should_flag_dictionary_as_writeable_collection()
		{
			typeof(Dictionary<string, int>).IsListOrDictionaryType().ShouldBeTrue();
		}
	}
}
#endif
