// The assertions AutoMapper 3.3.1's tests use, from the Should library they shipped with (ShouldEqual, ShouldBeType,
// ...), written here over xUnit's Assert with the same names and meanings. Not the Should library's code.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Should
{
    public static class ShouldExtensions
    {
        public static void ShouldEqual<T>(this T actual, T expected) => Assert.Equal(expected, actual);

        public static void ShouldEqual<T>(this T actual, T expected, string userMessage) => Assert.Equal(expected, actual);

        public static void ShouldEqual(this double actual, double expected, double tolerance) => Assert.Equal(expected, actual, tolerance);

        public static void ShouldEqual(this DateTime actual, DateTime expected, TimeSpan tolerance) => Assert.Equal(expected, actual, tolerance);

        public static void ShouldNotEqual<T>(this T actual, T expected) => Assert.NotEqual(expected, actual);

        public static void ShouldNotEqual<T>(this T actual, T expected, string userMessage) => Assert.NotEqual(expected, actual);

        public static void ShouldBeNull(this object @object) => Assert.Null(@object);

        public static T ShouldNotBeNull<T>(this T @object) where T : class
        {
            Assert.NotNull(@object);
            return @object;
        }

        public static T ShouldNotBeNull<T>(this T @object, string message) where T : class => @object.ShouldNotBeNull();

        public static void ShouldBeTrue(this bool condition) => Assert.True(condition);

        public static void ShouldBeTrue(this bool condition, string userMessage) => Assert.True(condition, userMessage);

        public static void ShouldBeFalse(this bool condition) => Assert.False(condition);

        public static void ShouldBeFalse(this bool condition, string userMessage) => Assert.False(condition, userMessage);

        public static void ShouldBeSameAs(this object actual, object expected) => Assert.Same(expected, actual);

        public static void ShouldNotBeSameAs(this object actual, object expected) => Assert.NotSame(expected, actual);

        public static T ShouldBeType<T>(this object @object) => Assert.IsType<T>(@object);

        public static void ShouldBeType(this object @object, Type expectedType) => Assert.IsType(expectedType, @object);

        public static void ShouldNotBeType<T>(this object @object) => Assert.IsNotType<T>(@object);

        public static void ShouldNotBeType(this object @object, Type expectedType) => Assert.IsNotType(expectedType, @object);

        public static T ShouldImplement<T>(this object @object) => Assert.IsAssignableFrom<T>(@object);

        public static void ShouldImplement(this object @object, Type expectedType) => Assert.IsAssignableFrom(expectedType, @object);

        public static void ShouldContain<T>(this IEnumerable<T> collection, T expected) => Assert.Contains(expected, collection);

        public static void ShouldNotContain<T>(this IEnumerable<T> collection, T expected) => Assert.DoesNotContain(expected, collection);

        public static void ShouldContain(this string actualString, string expectedSubString) => Assert.Contains(expectedSubString, actualString);

        public static void ShouldBeEmpty(this IEnumerable collection) => Assert.Empty(collection);

        public static void ShouldNotBeEmpty(this IEnumerable collection) => Assert.NotEmpty(collection);

        public static void ShouldBeGreaterThan<T>(this T @object, T value) where T : IComparable<T> => Assert.True(@object.CompareTo(value) > 0);

        public static void ShouldBeLessThan<T>(this T @object, T value) where T : IComparable<T> => Assert.True(@object.CompareTo(value) < 0);
    }

    public static class CollectionAssertExtensions
    {
        public static void ShouldContain<T>(this IEnumerable<T> collection, T expected) => Assert.Contains(expected, collection);
    }
}

namespace Should.Core.Exceptions
{
    public class AssertException : Exception
    {
        public AssertException(string message) : base(message)
        {
        }

        public AssertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

namespace Should.Core.Assertions
{
}
