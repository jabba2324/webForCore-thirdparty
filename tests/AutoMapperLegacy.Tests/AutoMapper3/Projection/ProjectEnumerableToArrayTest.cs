// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/ProjectEnumerableToArrayTest.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Projection
{
    using AutoMapper.QueryableExtensions;

    public class ProjectEnumerableToArrayTest
    {
        public ProjectEnumerableToArrayTest()
        {
            Mapper.CreateMap<Movie, MovieDto>();
            Mapper.CreateMap<Actor, ActorDto>();
        }

        [Fact]
        public void EnumerablesAreMappedToArrays()
        {
            var movies = 
                new List<Movie>() {
                new Movie() { Actors = new Actor[] { new Actor() { Name = "Actor 1" }, new Actor() { Name = "Actor 2" } } },
                new Movie() { Actors = new Actor[] { new Actor() { Name = "Actor 3" }, new Actor() { Name = "Actor 4" } } }
                }.AsQueryable();

            var mapped = movies.Project().To<MovieDto>();

            mapped.ElementAt(0).Actors.Length.ShouldEqual(2);
            mapped.ElementAt(1).Actors[1].Name.ShouldEqual("Actor 4");
        }

        public class Movie
        {
            public IEnumerable<Actor> Actors { get; set; }
        }

        public class MovieDto
        {
            public ActorDto[] Actors { get; set; }
        }

        public class Actor
        {
            public string Name { get; set; }
        }

        public class ActorDto
        {
            public string Name { get; set; }
        }
    }
}
