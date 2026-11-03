// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/AllMembersNullSubstitute.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
// using AutoMapper.Mappers; (AutoMapper 3.x's namespace)
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
	[PortSkip(Skips.Behaviour)]
	public class AllMembersNullSubstituteBug : AutoMapperSpecBase
	{
        public class Source
        {
            public int? Value1 { get; set; }
            public int? Value2 { get; set; }
            public int? Value3 { get; set; }
        }

        public class Destination
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
            public string Value3 { get; set; }
        }

		[Fact]
		public void Should_map_all_null_values_to_its_substitute()
		{
            Mapper.CreateMap<Source, Destination>()
                .ForAllMembers(opt => opt.NullSubstitute(string.Empty));

		    var src = new Source
		    {
		        Value1 = 5
		    };

		    var dest = Mapper.Map<Source, Destination>(src);

		    dest.Value1.ShouldEqual("5");
		    dest.Value2.ShouldEqual(string.Empty);
		    dest.Value3.ShouldEqual(string.Empty);
		}
	}
}
