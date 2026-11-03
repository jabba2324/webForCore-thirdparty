// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/ObjectTypeMapFailure.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Bug
{
    public class ObjectTypeMapFailure : NonValidatingSpecBase
    {
        [Fact]
        public void Should_map_the_object_type()
        {
            var displayModel = new DisplayModel
            {
                Radius = 300
            };
            object vm = new SomeViewModel();
            Mapper.CreateMap<DisplayModel, SomeViewModel>();

            Mapper.Map(displayModel, vm);
            ((SomeViewModel)vm).Radius.ShouldEqual(300); // fails

            var vm2 = new SomeViewModel();
            Mapper.Map(displayModel, vm2);
            vm2.Radius.ShouldEqual(300); // succeeds
        }

        public class SomeViewModel
        {
            public int Radius { get; set; }
        }

        public class DisplayModel
        {
            public int Radius { get; set; }
        }
    }
}
