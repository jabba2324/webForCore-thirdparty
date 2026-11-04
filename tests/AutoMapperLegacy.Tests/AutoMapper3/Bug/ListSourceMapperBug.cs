// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/ListSourceMapperBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
namespace AutoMapper3.UnitTests.Bug
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Should;
    using Assert = Xunit.Assert;

    public class ListSourceMapperBug
    {
        public class CustomCollection<T> : Collection<T>, IListSource
        {
            public IList GetList()
            {
                return new ReadOnlyCollection<T>(this.ToList());
            }

            public bool ContainsListCollection
            {
                get { return true; }
            }
        }

        public class Source
        {
        }

        public class Dest
        {
        }

        [Fact]
        public void CustomListSourceShouldNotBlowUp()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Source, Dest>());

            var source = new CustomCollection<Source> {new Source()};

            var dests = Mapper.Map<CustomCollection<Source>, CustomCollection<Dest>>(source);

            dests.Count.ShouldEqual(1);
        }
    }
}
