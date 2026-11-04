// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/SubclassMappings.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class SubclassMappings : AutoMapperSpecBase
    {
        public class Source
        {
            public string Name { get; set; }
        }

        public class Destination
        {
            public string Name { get; set; }
        }

        public class SubDestination : Destination
        {
            public string SubName { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Destination>();
        }

        [Fact]
        public void TestCase()
        {

            var source = new Source() { Name = "Test" };
            var destination = new Destination();

            Mapper.Map<Source, Destination>(source, destination); // Works

            var subDestination = new SubDestination();

            Mapper.Map<Source, Destination>(source, subDestination); // Fails
        }
    }
}
