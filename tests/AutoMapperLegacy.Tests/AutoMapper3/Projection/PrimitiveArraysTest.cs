// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Projection/PrimitiveArraysTest.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Should;

namespace AutoMapper3.UnitTests.Projection
{
    namespace PrimitiveArrays
    {
        using AutoMapper.QueryableExtensions;

        public class PrimitiveArraysExpressionTest
        {
            [Fact]
            public void Should_not_fail()
            {
                Mapper.CreateMap<Source, Destination>();

                typeof(NullReferenceException).ShouldNotBeThrownBy(() => Mapper.Engine.CreateMapExpression<Source, Destination>());
            }

            [Fact]
            public void Should_map_values()
            {
                Mapper.CreateMap<Source, Destination>();

                var sources = new List<Source>
                {
                    new Source
                    {
                        Value1 = new byte[] {1, 2, 3},
                        Value2 = new[] {4, 5, 6}
                    }
                };

                var expr = sources.AsQueryable().Project().To<Destination>();

                var result = expr.ToList();

                result[0].Value1.ShouldEqual(sources[0].Value1);
                result[0].Value2.ShouldEqual(sources[0].Value2);
            }
        }
        public class Source
        {
            public byte[] Value1 { get; set; }
            public int[] Value2 { get; set; }
        }

        public class Destination
        {
            public byte[] Value1 { get; set; }
            public int[] Value2 { get; set; }
        }
    }
}
