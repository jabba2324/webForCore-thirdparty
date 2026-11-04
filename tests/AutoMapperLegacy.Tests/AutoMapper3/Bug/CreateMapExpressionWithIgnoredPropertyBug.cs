// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/CreateMapExpressionWithIgnoredPropertyBug.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Bug
{
    using AutoMapper.QueryableExtensions;

    public class CreateMapExpressionWithIgnoredPropertyBug : NonValidatingSpecBase
    {
        [Fact]
        public void ShouldNotMapPropertyWhenItIsIgnored()
        {
            Mapper.CreateMap<Person, Person>()
                .ForMember(x => x.Name, x => x.Ignore());

            IQueryable<Person> collection = (new List<Person> { new Person { Name = "Person1" } }).AsQueryable();

            List<Person> result = collection.Project().To<Person>().ToList();

            result.ForEach(x => x.Name.ShouldBeNull());
        }

        public class Person
        {
            public string Name { get; set; }
        }
    }
}
