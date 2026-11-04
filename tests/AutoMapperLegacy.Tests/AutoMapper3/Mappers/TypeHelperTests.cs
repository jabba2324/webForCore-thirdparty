// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Mappers/TypeHelperTests.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
#if AM3_ENGINE // Not compiled against the facade; why: Compat/Porting.cs (Skips)
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
// using AutoMapper.Mappers; (AutoMapper 3.x's namespace)
using Should;
using Xunit;

namespace AutoMapper3.UnitTests.Mappers
{
    public class TypeHelperTests
    {
        [Fact]
        public void CanReturnElementTypeOnCollectionThatImplementsTheSameGenericInterfaceMultipleTimes()
        {
            Type myType = typeof(ChargeCollection);

            Type elementType = TypeHelper.GetElementType(myType);

            elementType.ShouldNotBeNull();
        }

        public class Charge { }

        public interface IChargeCollection : IEnumerable<object> { }

        public class ChargeCollection : Collection<Charge>, IChargeCollection
        {
            public new IEnumerator<object> GetEnumerator()
            {
                return null;
            }
        }
    }
}
#endif
