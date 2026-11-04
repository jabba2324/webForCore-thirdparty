// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/EFCollections.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Bug
{
    public class EFCollections : AutoMapperSpecBase
    {
        private Dest _dest;

        public class Source
        {
            public ICollection<Child> Children { get; set; }

        }

        public class _Source : Source
        {
        }

        public class _Child : Child
        {

        }

        public class Dest
        {
            public ICollection<DestChild> Children { get; set; } 
        }

        public class DestChild {}

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Dest>();
                cfg.CreateMap<Child, DestChild>();
            });
        }

        protected override void Because_of()
        {
            var source = new _Source
            {
                Children = new Collection<Child>
                {
                    new _Child(),
                    new _Child()
                }
            };
            _dest = Mapper.Map<Source, Dest>(source);
        }

        [Fact]
        public void Should_map_collection_items()
        {
            _dest.Children.Count.ShouldEqual(2);
        }
    }
}
