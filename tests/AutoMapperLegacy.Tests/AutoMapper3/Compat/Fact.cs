// The ported tests' [Fact], on xUnit 2, with a skip that can be put on a test's class (or an enclosing class) as well
// as on the test: AutoMapper 3.3.1's tests set up their maps in the constructor of a class with several facts, so a
// difference usually skips the whole class. This [Fact] is found before xUnit's in the AutoMapper3.UnitTests
// namespace, so the tests keep their attributes as they were written. Each test starts with a reset Mapper: most of
// them reset it when they finish (SpecBase), and the others relied on the order they ran in.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AutoMapper3.UnitTests
{
    [XunitTestCaseDiscoverer("AutoMapper3.UnitTests.SkippableFactDiscoverer", "AutoMapperLegacy.Tests")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FactAttribute : Xunit.FactAttribute
    {
    }

    /// <summary>Skips the test, or every test of the class and its nested classes, with one of <see cref="Skips"/>.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class PortSkipAttribute : Attribute
    {
        public PortSkipAttribute(string reason) => Reason = reason;

        public string Reason { get; }
    }

    public sealed class SkippableFactDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public SkippableFactDiscoverer(IMessageSink diagnosticMessageSink) => _diagnosticMessageSink = diagnosticMessageSink;

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            yield return new SkippableTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);
        }
    }

    public sealed class SkippableTestCase : XunitTestCase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public SkippableTestCase()
        {
        }

        public SkippableTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
        {
        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments,
            ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            Mapper.Reset();
            return base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);
        }

        protected override string GetSkipReason(IAttributeInfo factAttribute)
        {
            var reason = base.GetSkipReason(factAttribute);
            if (reason != null)
                return reason;
            // The test's own types (reflection over this assembly's test classes): the method, then its class, the
            // class's base classes and the classes that enclose them.
            var method = (TestMethod.Method as IReflectionMethodInfo)?.MethodInfo;
            var skip = method?.GetCustomAttribute<PortSkipAttribute>();
            if (skip != null)
                return skip.Reason;
            for (var type = (TestMethod.TestClass.Class as IReflectionTypeInfo)?.Type; type != null; type = type.DeclaringType)
            {
                skip = type.GetCustomAttribute<PortSkipAttribute>(inherit: true);
                if (skip != null)
                    return skip.Reason;
            }
            return null;
        }
    }
}
