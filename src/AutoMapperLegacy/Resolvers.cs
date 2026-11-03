// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// ValueResolver<,>, TypeConverter<,>, IValueResolver and ResolutionResult are adapted from AutoMapper 3.3.1
// (ValueResolver.cs, ITypeConverter.cs, IValueResolver.cs, ResolutionResult.cs), Copyright (c) 2010 Jimmy Bogard,
// MIT License (see NOTICE).
using System;
using System.Linq.Expressions;
using AutoMapper;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x's value resolver: <c>opt.ResolveUsing(resolver)</c> calls <see cref="Resolve"/> with the source
    /// object (or the member <c>FromMember</c> names) and maps the result to the destination member's type.
    /// </summary>
    public interface IValueResolver
    {
        ResolutionResult Resolve(ResolutionResult source);
    }

    /// <summary>
    /// AutoMapper 3.x's base class for value resolvers: override <see cref="ResolveCore"/>.
    /// </summary>
    public abstract class ValueResolver<TSource, TDestination> : IValueResolver
    {
        public ResolutionResult Resolve(ResolutionResult source)
        {
            if (source.Value != null && !(source.Value is TSource))
            {
                throw new AutoMapperMappingException(string.Format(
                    "Value supplied is of type {0} but expected {1}.\nChange the value resolver source type, or redirect the source value supplied to the value resolver using FromMember.",
                    source.Value.GetType(), typeof(TSource)));
            }

            return source.New(ResolveCore((TSource)source.Value), typeof(TDestination));
        }

        protected abstract TDestination ResolveCore(TSource source);
    }

    /// <summary>
    /// The value a resolver received or returned, with the context (AutoMapper 3.x's <c>ResolutionResult</c>).
    /// </summary>
    public class ResolutionResult
    {
        /// <summary>A result for the context's source value (<c>context.SourceValue</c>).</summary>
        public ResolutionResult(ResolutionContext context)
            : this(context.SourceValue, context, context.SourceType)
        {
        }

        internal ResolutionResult(object value, ResolutionContext context, Type memberType)
        {
            Value = value;
            Context = context;
            MemberType = memberType;
            Type = value != null ? value.GetType() : memberType;
        }

        public object Value { get; }

        /// <summary>The value's runtime type, or the member type if the value is null.</summary>
        public Type Type { get; }

        public Type MemberType { get; }

        public ResolutionContext Context { get; }

        /// <summary>When set, the destination member is left as it is.</summary>
        public bool ShouldIgnore { get; set; }

        public ResolutionResult Ignore() => new ResolutionResult(null, Context, MemberType) { ShouldIgnore = true };

        public ResolutionResult New(object value) => new ResolutionResult(value, Context, value != null ? value.GetType() : MemberType);

        public ResolutionResult New(object value, Type memberType) => new ResolutionResult(value, Context, memberType);
    }

    /// <summary>
    /// AutoMapper 3.x's type converter, which takes the whole context. AutoMapper 12 has an interface of the same name
    /// (<c>AutoMapper.ITypeConverter&lt;,&gt;</c>, whose <c>Convert</c> takes the source, the destination and the
    /// context): a class that implements this one directly must name it <c>AutoMapperLegacy.ITypeConverter&lt;,&gt;</c>
    /// in a file that also has <c>using AutoMapper;</c>. Classes derived from <see cref="TypeConverter{TSource,TDestination}"/>
    /// need no change.
    /// </summary>
    public interface ITypeConverter<in TSource, out TDestination>
    {
        TDestination Convert(ResolutionContext context);
    }

    /// <summary>
    /// AutoMapper 3.x's base class for type converters: override <see cref="ConvertCore"/>.
    /// </summary>
    public abstract class TypeConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination>
    {
        public TDestination Convert(ResolutionContext context)
        {
            var source = context.SourceValue;
            if (source != null && !(source is TSource))
            {
                throw new AutoMapperMappingException(string.Format(
                    "Value supplied is of type {0} but expected {1}.\nChange the type converter source type, or redirect the source value supplied to the value resolver using FromMember.",
                    typeof(TSource), source.GetType()));
            }

            return ConvertCore((TSource)source);
        }

        protected abstract TDestination ConvertCore(TSource source);
    }

    /// <summary>
    /// AutoMapper 3.x's before/after map action (<c>BeforeMap&lt;TAction&gt;()</c>, <c>AfterMap&lt;TAction&gt;()</c>),
    /// whose <c>Process</c> takes no context. AutoMapper 12 has an interface of the same name with a context: a class
    /// that implements this one must name it <c>AutoMapperLegacy.IMappingAction&lt;,&gt;</c> in a file that also has
    /// <c>using AutoMapper;</c>.
    /// </summary>
    public interface IMappingAction<in TSource, in TDestination>
    {
        void Process(TSource source, TDestination destination);
    }

    /// <summary>Redirects a resolver's source (3.x <c>ResolveUsing(...).FromMember(...)</c>).</summary>
    public interface IResolutionExpression
    {
        void FromMember(string sourcePropertyName);
    }

    public interface IResolutionExpression<TSource> : IResolutionExpression
    {
        void FromMember(Expression<Func<TSource, object>> sourceMember);
    }

    public interface IResolverConfigurationExpression : IResolutionExpression
    {
        IResolutionExpression ConstructedBy(Func<IValueResolver> constructor);
    }

    public interface IResolverConfigurationExpression<TSource> : IResolutionExpression<TSource>
    {
        IResolutionExpression<TSource> ConstructedBy(Func<IValueResolver> constructor);
    }

    public interface IResolverConfigurationExpression<TSource, TValueResolver>
        where TValueResolver : IValueResolver
    {
        IResolverConfigurationExpression<TSource, TValueResolver> FromMember(Expression<Func<TSource, object>> sourceMember);
        IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName);
        IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor);
    }
}
