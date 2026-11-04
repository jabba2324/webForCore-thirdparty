// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/InterfaceSelfMappingBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Bug
{
    public class InterfaceSelfMappingBug
    {
        public interface IFoo
        {
            int Value { get; set; } 
        }

        public class Bar : IFoo
        {
            public int Value { get; set; }
        }

        public class Baz : IFoo
        {
            public int Value { get; set; }
        }

        [Fact]
        public void Example()
        {
            Mapper.Configuration.AllowNullCollections = true;
            Mapper.CreateMap<IFoo, IFoo>();
            Mapper.AssertConfigurationIsValid();

            IFoo bar = new Bar
            {
                Value = 5
            };
            IFoo baz = new Baz
            {
                Value = 10
            };

            Mapper.Map(bar, baz);

            baz.Value.ShouldEqual(5);
        }
    }
}
