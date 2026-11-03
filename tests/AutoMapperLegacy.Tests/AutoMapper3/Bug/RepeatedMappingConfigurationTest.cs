// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/RepeatedMappingConfigurationTest.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
	public class When_mapping_for_derived_class_is_duplicated : AutoMapperSpecBase
	{
		public class ModelObject
		{
			public string BaseString { get; set; }
		}

		public class ModelSubObject : ModelObject
		{
			public string SubString { get; set; }
		}

		public class DtoObject
		{
			public string BaseString { get; set; }
		}

		public class DtoSubObject : DtoObject
		{
			public string SubString { get; set; }
		}

		[Fact]
		public void should_not_throw_duplicated_key_exception()
		{
			Mapper.CreateMap<ModelSubObject, DtoObject>()
				.Include<ModelSubObject, DtoSubObject>();

			Mapper.CreateMap<ModelSubObject, DtoSubObject>();

			Mapper.CreateMap<ModelSubObject, DtoObject>()
				.Include<ModelSubObject, DtoSubObject>();

			Mapper.CreateMap<ModelSubObject, DtoSubObject>();
		}
	}
}
