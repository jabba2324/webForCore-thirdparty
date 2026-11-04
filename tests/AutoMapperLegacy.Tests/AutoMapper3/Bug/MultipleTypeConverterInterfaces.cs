// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/MultipleTypeConverterInterfaces.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
		public class When_specifying_a_type_converter_implementing_multiple_type_converter_interfaces : SpecBase
		{
			private DestinationFoo _resultFoo;
			private DestinationBar _resultBar;

			public class SourceFoo
			{
				public int SourceFooValue { get; set; }
			}

			public class DestinationFoo
			{
				public int DestinationFooValue { get; set; }
			}
			public class SourceBar
			{
				public int SourceBarValue { get; set; }
			}

			public class DestinationBar
			{
				public int DestinationBarValue { get; set; }
			}

			public class DualConverter : AutoMapperLegacy.ITypeConverter<SourceFoo, DestinationFoo>,
										 AutoMapperLegacy.ITypeConverter<SourceBar, DestinationBar>
			{
				public DestinationFoo Convert(ResolutionContext context)
				{
					var source = context.SourceValue as SourceFoo;
					return new DestinationFoo { DestinationFooValue = source.SourceFooValue + 100 };
				}

				DestinationBar AutoMapperLegacy.ITypeConverter<SourceBar, DestinationBar>.Convert(ResolutionContext context)
				{
					var source = context.SourceValue as SourceBar;
					return new DestinationBar { DestinationBarValue = source.SourceBarValue + 1000 };
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap(typeof(SourceFoo), typeof(DestinationFoo)).ConvertUsing(typeof(DualConverter));
				Mapper.CreateMap(typeof(SourceBar), typeof(DestinationBar)).ConvertUsing(typeof(DualConverter));
			}

			protected override void Because_of()
			{
				_resultFoo = Mapper.Map<SourceFoo, DestinationFoo>(new SourceFoo { SourceFooValue = 5 });
				_resultBar = Mapper.Map<SourceBar, DestinationBar>(new SourceBar { SourceBarValue = 6 });
			}

			[Fact]
			public void Should_use_implicit_converter()
			{
				_resultFoo.DestinationFooValue.ShouldEqual(105);
			}

			[Fact]
			public void Should_use_explicit_converter()
			{
				_resultBar.DestinationBarValue.ShouldEqual(1006);
			}

			[Fact]
			public void Should_pass_configuration_validation()
			{
				Mapper.AssertConfigurationIsValid();
			}
		}
}
