// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/DuplicateValuesBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System.Collections.Generic;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Bug
{
	namespace DuplicateValuesBug
	{
		public class SourceObject
		{
			public int Id;
			public IList<SourceObject> Children;

			public void AddChild(SourceObject childObject)
			{
				if (this.Children == null)
					this.Children = new List<SourceObject>();

				Children.Add(childObject);
			}
		}


		public class DestObject
		{
			public int Id;
			public IList<DestObject> Children;

			public void AddChild(DestObject childObject)
			{
				if (this.Children == null)
					this.Children = new List<DestObject>();

				Children.Add(childObject);
			}
		}
		public class DuplicateValuesIssue : AutoMapperSpecBase
		{
			[Fact]
			public void Should_map_the_existing_array_elements_over()
			{
				var sourceList = new List<SourceObject>();
				var destList = new List<DestObject>();

				Mapper.CreateMap<SourceObject, DestObject>();
				Mapper.AssertConfigurationIsValid();

				var source1 = new SourceObject
				{
					Id = 1,
				};
				sourceList.Add(source1);

				var source2 = new SourceObject
				{
					Id = 2,
				};
				sourceList.Add(source2);

				source1.AddChild(source2); // This causes the problem

				DestObject dest1 = new DestObject();
				Mapper.Map(sourceList, destList);

				destList.Count.ShouldEqual(2);
				destList[0].Children.Count.ShouldEqual(1);
				destList[0].Children[0].ShouldBeSameAs(destList[1]);
			}
		}
	}
}
