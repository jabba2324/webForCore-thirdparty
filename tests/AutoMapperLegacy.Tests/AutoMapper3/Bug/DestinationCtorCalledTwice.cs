// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/Bug/DestinationCtorCalledTwice.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
#if AM3_REMOVED // Not compiled against the facade; why: Compat/Porting.cs (Skips)
using AutoMapper;
using Xunit;
using Should;

namespace AutoMapper3.UnitTests.Bug
{
    namespace DestinationCtorCalledTwice
    {
        public class Bug : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }
            public class Destination
            {
                private static int _callCount = 0;
                
                public Destination()
                {
                    _callCount++;
                }

                public int Value { get; set; }
                public static int CallCount { get { return _callCount; } }

                public static void Reset()
                {
                    _callCount = 0;
                }
            }

            public Bug()
            {
                Destination.Reset();
            }

            [Fact]
            public void Should_call_ctor_once()
            {
                var source = new Source {Value = 5};
                var dest = new Destination {Value = 7};

                Mapper.Map(source, dest, opt => opt.CreateMissingTypeMaps = true);

                Destination.CallCount.ShouldEqual(1);
            }
        }
    }
}
#endif
