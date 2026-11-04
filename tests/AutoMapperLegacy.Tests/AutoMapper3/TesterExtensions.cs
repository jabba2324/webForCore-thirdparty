// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/TesterExtensions.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Should;

namespace AutoMapper3.UnitTests
{
	public static class StopgapNBehaveExtensions
	{
		public static void ShouldBeOfLength<T>(this IEnumerable<T> collection, int length)
		{
			collection.ShouldNotBeNull();
			collection.Count().ShouldEqual(length);
		}
	}
}
