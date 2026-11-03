// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/MultipleInterfaceInheritance.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    public class MultipleInterfaceInheritance : AutoMapperSpecBase
    {
        private ThingDto _thingDto;

        public class Thing
        {
            public IItem[] Items { get; set; }
        }

        public class ThingDto
        {
            public ItemDto[] Items { get; set; }
        }

        public class Item : IItem
        {
        }

        public class ItemDto
        {
        }

        public interface IItem : ISome     // everything works well if IItem doesn't inherit ISome.
        {
        }

        public interface ISome
        {
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Thing, ThingDto>();
                cfg.CreateMap<IItem, ItemDto>();
            });
        }

        protected override void Because_of()
        {
            var thing = new Thing { Items = new[] { new Item() } };
            _thingDto = Mapper.Map<Thing, ThingDto>(thing);
        }

        [Fact]
        public void Should_map_successfully()
        {
            _thingDto.Items.Length.ShouldEqual(1);
        }
    }
}
