// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 and 4.2.1, Copyright (c) 2010 Jimmy Bogard
// (MIT License; see NOTICE).
using System;
using System.Collections.Generic;
using AutoMapper;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x's members of <see cref="ResolutionContext"/> (<c>SourceValue</c>, <c>DestinationValue</c>,
    /// <c>Options.Items</c>, <c>Engine</c>, ...), as extension properties on AutoMapper 12's <see cref="ResolutionContext"/>,
    /// so that converters and resolvers written for 3.x compile unchanged. They need C# 14 (the default for
    /// <c>net10.0</c>).
    /// </summary>
    /// <remarks>
    /// AutoMapper 12's context doesn't carry the value being mapped. The facade passes it in when it calls 3.x code (a
    /// <see cref="ITypeConverter{TSource,TDestination}"/>, an <see cref="IValueResolver"/>, a <c>Condition</c> or
    /// <c>ConstructUsing</c> over the context): <c>SourceValue</c>, <c>DestinationValue</c>, <c>SourceType</c>,
    /// <c>DestinationType</c> and <c>MemberName</c> are available while that code runs, on the thread that runs it, and
    /// throw <see cref="InvalidOperationException"/> anywhere else. 3.x's <c>Parent</c>, <c>TypeMap</c>,
    /// <c>PropertyMap</c> and <c>ArrayIndex</c> aren't available.
    /// </remarks>
    public static class ResolutionContextExtensions
    {
        extension(ResolutionContext context)
        {
            /// <summary>The source value being mapped (3.x <c>ResolutionContext.SourceValue</c>).</summary>
            public object SourceValue => LegacyFrame.Require(context).SourceValue;

            /// <summary>The destination value, if there is one yet (3.x <c>ResolutionContext.DestinationValue</c>).</summary>
            public object DestinationValue => LegacyFrame.Require(context).DestinationValue;

            /// <summary>The source type (the runtime type of the source value, if it isn't null).</summary>
            public Type SourceType => LegacyFrame.Require(context).SourceType;

            /// <summary>The destination type.</summary>
            public Type DestinationType => LegacyFrame.Require(context).DestinationType;

            /// <summary>The destination member's name, when a member is being resolved.</summary>
            public string MemberName => LegacyFrame.Require(context).MemberName;

            /// <summary>Whether <c>SourceValue</c> is null.</summary>
            public bool IsSourceValueNull => LegacyFrame.Require(context).SourceValue == null;

            /// <summary>The mapping operation's options: <c>Items</c> holds what the <c>Map</c> call's options put there.</summary>
            public MappingOperationOptions Options => new MappingOperationOptions(context);

            /// <summary>An engine that maps within this operation (it shares <c>Options.Items</c>).</summary>
            public IMappingEngine Engine => new RuntimeMappingEngine(context.Mapper);
        }
    }

    /// <summary>
    /// The options of a mapping operation, as AutoMapper 3.x's <c>ResolutionContext.Options</c> gave them.
    /// </summary>
    public sealed class MappingOperationOptions
    {
        private static readonly IDictionary<string, object> NoItems = new Dictionary<string, object>();

        internal MappingOperationOptions(ResolutionContext context)
        {
            // Every Map call the facade makes passes options, so the items exist; an operation started without them
            // (AutoMapper 12's own IMapper.Map without options) has none, and they are empty here.
            Items = context.TryGetItems(out var items) ? items : NoItems;
        }

        /// <summary>The items passed in the options of the <c>Map</c> call.</summary>
        public IDictionary<string, object> Items { get; }
    }

    /// <summary>
    /// What 3.x code sees of the value being mapped while it runs: a stack per thread, one frame for each call into 3.x
    /// code, keyed by AutoMapper 12's context (one context can be shared by several mapping operations at once on
    /// different threads, so it can't hold the values itself).
    /// </summary>
    internal sealed class LegacyFrame
    {
        [ThreadStatic] private static LegacyFrame _top;

        private LegacyFrame Previous;
        private ResolutionContext Context;
        public object SourceValue;
        public object DestinationValue;
        public Type SourceType;
        public Type DestinationType;
        public string MemberName;

        public static T Run<T>(ResolutionContext context, object source, Type sourceType, object destination,
            Type destinationType, string memberName, Func<T> body)
        {
            var frame = new LegacyFrame
            {
                Previous = _top,
                Context = context,
                SourceValue = source,
                SourceType = source?.GetType() ?? sourceType,
                DestinationValue = destination,
                DestinationType = destinationType,
                MemberName = memberName,
            };
            _top = frame;
            try
            {
                return body();
            }
            finally
            {
                _top = frame.Previous;
            }
        }

        public static LegacyFrame Find(ResolutionContext context)
        {
            for (var frame = _top; frame != null; frame = frame.Previous)
            {
                if (ReferenceEquals(frame.Context, context))
                    return frame;
            }
            return null;
        }

        public static LegacyFrame Require(ResolutionContext context) =>
            Find(context) ?? throw new InvalidOperationException(
                "ResolutionContext.SourceValue, DestinationValue, SourceType, DestinationType and MemberName are only " +
                "available while AutoMapperLegacy calls 3.x code (a TypeConverter, a ValueResolver, a Condition or " +
                "ConstructUsing over the context), on the thread that runs it. AutoMapper 12's context doesn't carry them.");
    }
}
