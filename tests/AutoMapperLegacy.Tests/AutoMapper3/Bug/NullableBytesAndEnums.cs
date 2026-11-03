// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/NullableBytesAndEnums.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
	public class NullableBytesAndEnums : AutoMapperSpecBase
	{
		private Destination _destination;

		public class Source
		{
			public byte? Value { get; set; }
		}

		public enum Foo : byte
		{
			Blarg = 1,
			Splorg = 2
		}

		public class Destination
		{
			public Foo? Value { get; set; }
		}

		protected override void Establish_context()
		{
			Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<Source, Destination>();
			});
		}

		protected override void Because_of()
		{
			_destination = Mapper.Map<Source, Destination>(new Source {Value = 2});
		}

		[Fact]
		public void Should_map_the_byte_to_the_enum_with_the_same_value()
		{
			_destination.Value.ShouldEqual(Foo.Splorg);
		}
	}

#if NET40
    public class NullableLong : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public long? Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Value = 2 });
        }

        [Fact]
        public void Should_map_the_byte_to_the_enum_with_the_same_value()
        {
            _destination.Value.ShouldEqual(2);
        }
    }
#endif
}
