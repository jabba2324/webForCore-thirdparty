// Ported from AutoMapper 3.3.1's unit tests, src/UnitTests/AutoMapperSpecBase.cs
// (https://github.com/LuckyPennySoftware/AutoMapper/tree/v3.3.1), Copyright (c) 2010 Jimmy Bogard, MIT License (see
// NOTICE). Changed for webForCore.AutoMapperLegacy as described in AutoMapper3/Compat/Porting.cs.
using AutoMapper;
using System;
using Should;
using Xunit;
#if RHINO_MOCKS
#if RHINO_MOCKS
using Rhino.Mocks;
#endif
#endif

namespace AutoMapper3.UnitTests
{
    public class AutoMapperSpecBase : NonValidatingSpecBase
    {
        [Fact]
        public void Should_have_valid_configuration()
        {
            Mapper.AssertConfigurationIsValid();
        }
    }

    public class NonValidatingSpecBase : SpecBase
    {
        protected override void Cleanup()
        {
            Mapper.Reset();
        }

    }

    public abstract class SpecBaseBase
    {
        public virtual void MainSetup()
        {
            Establish_context();
            Because_of();
        }

        public virtual void MainTeardown()
        {
            Cleanup();
        }

        protected virtual void Establish_context()
        {
        }

        protected virtual void Because_of()
        {
        }

        protected virtual void Cleanup()
        {
        }

#if RHINO_MOCKS
        protected TType CreateDependency<TType>()
            where TType : class
        {
            return MockRepository.GenerateMock<TType>();
        }

        protected TType CreateStub<TType>() where TType : class
        {
            return MockRepository.GenerateStub<TType>();
        }
#endif
    }
    public abstract class SpecBase : SpecBaseBase, IDisposable
    {
        protected SpecBase()
        {
            MainSetup();
        }

        public override void MainSetup()
        {
            base.MainSetup();
        }

        public override void MainTeardown()
        {
            base.MainTeardown();
        }

        public void Dispose()
        {
            MainTeardown();
        }
    }

}

