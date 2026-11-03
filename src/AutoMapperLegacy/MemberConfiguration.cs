// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 and 4.2.1 (IMemberConfigurationExpression),
// Copyright (c) 2010 Jimmy Bogard, MIT License (see NOTICE).
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;

namespace AutoMapperLegacy
{
    /// <summary>
    /// The options of one destination member, as <c>ForMember</c> gives them to its lambda: AutoMapper 3.x's members
    /// (<c>ResolveUsing</c>, <c>UseValue</c>, <c>Condition</c> over the context, ...) and AutoMapper 12's, on AutoMapper
    /// 12's member configuration (<see cref="Inner"/>).
    /// </summary>
    public class LegacyMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        internal LegacyMemberConfigurationExpression(IMemberConfigurationExpression<TSource, TDestination, TMember> inner)
        {
            Inner = inner;
        }

        /// <summary>AutoMapper 12's member configuration.</summary>
        public IMemberConfigurationExpression<TSource, TDestination, TMember> Inner { get; }

        public MemberInfo DestinationMember => Inner.DestinationMember;

        // AutoMapper 3.x

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember) => Inner.MapFrom(sourceMember);

        public void MapFrom(string sourceMember) => Inner.MapFrom(sourceMember);

        /// <summary>4.x's <c>MapFrom&lt;TMember&gt;(string)</c>: the member's type argument isn't used.</summary>
        public void MapFrom<TSourceMember>(string sourceMember) => Inner.MapFrom(sourceMember);

        public IResolverConfigurationExpression<TSource, TValueResolver> ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
        {
            var resolver = new ResolverConfiguration<TSource>(typeof(TValueResolver));
            LegacyMembers.Resolve(Inner, resolver);
            return new ResolverConfiguration<TSource, TValueResolver>(resolver);
        }

        public IResolverConfigurationExpression<TSource> ResolveUsing(Type valueResolverType) => Inner.ResolveUsing(valueResolverType);

        public IResolutionExpression<TSource> ResolveUsing(IValueResolver valueResolver) => Inner.ResolveUsing(valueResolver);

        public void ResolveUsing(Func<TSource, object> resolver) => Inner.ResolveUsing(resolver);

        public void ResolveUsing(Func<ResolutionResult, object> resolver) => Inner.ResolveUsing(resolver);

        public void ResolveUsing(Func<ResolutionResult, TSource, object> resolver) => Inner.ResolveUsing(resolver);

        public void Ignore() => Inner.Ignore();

        public void UseDestinationValue() => Inner.UseDestinationValue();

        public void DoNotUseDestinationValue() => Inner.DoNotUseDestinationValue();

        public void UseValue<TValue>(TValue value) => Inner.UseValue(value);

        public void UseValue(object value) => Inner.UseValue<TSource, TDestination, TMember, object>(value);

        public void NullSubstitute(object nullSubstitute) => Inner.NullSubstitute(nullSubstitute);

        public void SetMappingOrder(int mappingOrder) => Inner.SetMappingOrder(mappingOrder);

        /// <summary>
        /// 3.x's condition over the source object. 3.x resolved the value first but threw a resolver's exception only
        /// when the condition held; AutoMapper 12 would throw it anyway, so the condition is checked before resolving
        /// (AutoMapper 12's <c>PreCondition</c>), which gives the same result.
        /// </summary>
        public void Condition(Func<TSource, bool> condition) => Inner.PreCondition(condition);

        public void Condition(Func<ResolutionContext, bool> condition) => Inner.Condition(condition);

        public void PreCondition(Func<TSource, bool> condition) => Inner.PreCondition(condition);

        public void PreCondition(Func<ResolutionContext, bool> condition) => LegacyMembers.PreCondition(Inner, condition);

        public void ExplicitExpansion() => LegacyMembers.ExplicitExpansion(Inner);

        // AutoMapper 12, for code that already uses it

        public void MapFrom<TResult>(Func<TSource, TDestination, TResult> mappingFunction) => Inner.MapFrom(mappingFunction);

        public void MapFrom<TResult>(Func<TSource, TDestination, TMember, TResult> mappingFunction) => Inner.MapFrom(mappingFunction);

        public void MapFrom<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> mappingFunction) => Inner.MapFrom(mappingFunction);

        public void MapFrom(IValueResolver<TSource, TDestination, TMember> valueResolver) => Inner.MapFrom(valueResolver);

        public void Condition(Func<TSource, TDestination, bool> condition) => Inner.Condition(condition);

        public void Condition(Func<TSource, TDestination, TMember, bool> condition) => Inner.Condition(condition);

        public void Condition(Func<TSource, TDestination, TMember, TMember, bool> condition) => Inner.Condition(condition);

        public void Condition(Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool> condition) => Inner.Condition(condition);

        public void PreCondition(Func<TSource, ResolutionContext, bool> condition) => Inner.PreCondition(condition);

        public void PreCondition(Func<TSource, TDestination, ResolutionContext, bool> condition) => Inner.PreCondition(condition);

        public void AllowNull() => Inner.AllowNull();

        public void DoNotAllowNull() => Inner.DoNotAllowNull();

        public void MapAtRuntime() => Inner.MapAtRuntime();

        public void AddTransform(Expression<Func<TMember, TMember>> transformer) => Inner.AddTransform(transformer);
    }

    /// <summary>
    /// The options of a member named by a string, for <c>CreateMap(Type, Type)</c> (AutoMapper 3.x's non-generic
    /// <c>IMemberConfigurationExpression</c>).
    /// </summary>
    public class LegacyMemberConfigurationExpression : LegacyMemberConfigurationExpression<object, object, object>
    {
        internal LegacyMemberConfigurationExpression(IMemberConfigurationExpression inner) : base(inner)
        {
            NonGenericInner = inner;
        }

        /// <summary>AutoMapper 12's member configuration.</summary>
        public IMemberConfigurationExpression NonGenericInner { get; }

        public new IResolverConfigurationExpression ResolveUsing(Type valueResolverType)
        {
            var resolver = new ResolverConfiguration<object>(valueResolverType);
            LegacyMembers.Resolve(Inner, resolver);
            return resolver;
        }

        public new IResolverConfigurationExpression ResolveUsing<TValueResolver>() where TValueResolver : IValueResolver
            => ResolveUsing(typeof(TValueResolver));

        public new IResolutionExpression ResolveUsing(IValueResolver valueResolver)
        {
            var resolver = new ResolverConfiguration<object>(valueResolver);
            LegacyMembers.Resolve(Inner, resolver);
            return resolver;
        }
    }

    /// <summary>
    /// AutoMapper 3.x's member options that AutoMapper 12 removed, as extension methods on AutoMapper 12's member
    /// configuration: for <c>ForMember</c> calls made on AutoMapper 12's <c>IMappingExpression&lt;,&gt;</c> (an
    /// application's own extension methods over it, for example). <c>ResolveUsing&lt;TValueResolver&gt;()</c> needs
    /// <c>ForMember</c> on the facade's <see cref="LegacyMappingExpression{TSource,TDestination}"/>, because an extension
    /// method can't be called with only its last type argument.
    /// </summary>
    public static class MemberConfigurationExpressionExtensions
    {
        public static IResolverConfigurationExpression<TSource> ResolveUsing<TSource, TDestination, TMember>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> options, Type valueResolverType)
        {
            var resolver = new ResolverConfiguration<TSource>(valueResolverType);
            LegacyMembers.Resolve(options, resolver);
            return resolver;
        }

        public static IResolutionExpression<TSource> ResolveUsing<TSource, TDestination, TMember>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> options, IValueResolver valueResolver)
        {
            var resolver = new ResolverConfiguration<TSource>(valueResolver);
            LegacyMembers.Resolve(options, resolver);
            return resolver;
        }

        public static void ResolveUsing<TSource, TDestination, TMember>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> options, Func<TSource, object> resolver)
            => LegacyMembers.MapFromFunction(options, (source, context) => resolver(source));

        public static void ResolveUsing<TSource, TDestination, TMember>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> options, Func<ResolutionResult, object> resolver)
            => LegacyMembers.MapFromFunction(options, (source, context) =>
                resolver(new ResolutionResult(source, context, typeof(TSource))));

        public static void ResolveUsing<TSource, TDestination, TMember>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> options, Func<ResolutionResult, TSource, object> resolver)
            => LegacyMembers.MapFromFunction(options, (source, context) =>
                resolver(new ResolutionResult(source, context, typeof(TSource)), source));

        public static void UseValue<TSource, TDestination, TMember, TValue>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> options, TValue value)
        {
            // 3.x mapped the value to the member's type, as AutoMapper 12 does with a function's result.
            Func<TSource, TDestination, TValue> constant = (source, destination) => value;
            options.MapFrom(constant);
        }

        /// <summary>
        /// 3.x's condition over the member's context: <c>SourceValue</c> is the source member's value (as 12 resolved
        /// it), <c>DestinationValue</c> the destination member's.
        /// </summary>
        public static void Condition<TSource, TDestination, TMember>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> options, Func<ResolutionContext, bool> condition)
        {
            var memberName = options.DestinationMember?.Name;
            Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool> wrapped =
                (source, destination, sourceMember, destinationMember, context) =>
                    LegacyFrame.Run(context, sourceMember, typeof(TMember), destinationMember, typeof(TMember), memberName,
                        () => condition(context));
            options.Condition(wrapped);
        }
    }

    /// <summary>The implementations of the 3.x member options.</summary>
    internal static class LegacyMembers
    {
        /// <summary>
        /// Maps the member from a function of the source object, as 3.x's resolvers did: the result is mapped to the
        /// member's type when it isn't one already (3.x passed it through the engine), and a 3.x frame is open while
        /// the function runs.
        /// </summary>
        public static void MapFromFunction<TSource, TDestination, TMember>(
            IMemberConfigurationExpression<TSource, TDestination, TMember> options, Func<TSource, ResolutionContext, object> function)
        {
            var memberName = options.DestinationMember?.Name;
            Func<TSource, TDestination, TMember, ResolutionContext, TMember> resolve =
                (source, destination, destinationMember, context) =>
                    LegacyFrame.Run(context, source, typeof(TSource), destination, typeof(TDestination), memberName,
                        () => ToMember<TMember>(function(source, context), context));
            options.MapFrom(resolve);
        }

        public static void Resolve<TSource, TDestination, TMember>(
            IMemberConfigurationExpression<TSource, TDestination, TMember> options, ResolverConfiguration<TSource> resolver)
        {
            var memberName = options.DestinationMember?.Name;
            Func<TSource, TDestination, TMember, ResolutionContext, TMember> resolve =
                (source, destination, destinationMember, context) =>
                    LegacyFrame.Run(context, source, typeof(TSource), destination, typeof(TDestination), memberName, () =>
                    {
                        var input = resolver.GetSource(source);
                        var result = resolver.CreateResolver().Resolve(new ResolutionResult(input, context, resolver.SourceType));
                        return result.ShouldIgnore ? destinationMember : ToMember<TMember>(result.Value, context);
                    });
            options.MapFrom(resolve);
        }

        public static void PreCondition<TSource, TDestination, TMember>(
            IMemberConfigurationExpression<TSource, TDestination, TMember> options, Func<ResolutionContext, bool> condition)
        {
            var memberName = options.DestinationMember?.Name;
            Func<TSource, TDestination, ResolutionContext, bool> wrapped = (source, destination, context) =>
                LegacyFrame.Run(context, source, typeof(TSource), destination, typeof(TDestination), memberName,
                    () => condition(context));
            options.PreCondition(wrapped);
        }

        /// <summary>
        /// <c>ExplicitExpansion()</c> became <c>ExplicitExpansion(bool value = true)</c> in AutoMapper 13. This package
        /// is built against 12, so the call only links there; with 13 and 14 it calls their public
        /// <c>ExplicitExpansion(bool)</c> by reflection, the one member of AutoMapper's public API it can't call directly
        /// on every version of the range.
        /// </summary>
        public static void ExplicitExpansion<TSource, TDestination, TMember>(IMemberConfigurationExpression<TSource, TDestination, TMember> options)
        {
            try
            {
                CallExplicitExpansion(options);
            }
            catch (MissingMethodException)
            {
                var method = typeof(IProjectionMemberConfiguration<TSource, TDestination, TMember>)
                    .GetMethod(nameof(IProjectionMemberConfiguration<TSource, TDestination, TMember>.ExplicitExpansion), new[] { typeof(bool) })
                    ?? throw new NotSupportedException("This AutoMapper has neither ExplicitExpansion() nor ExplicitExpansion(bool).");
                method.Invoke(options, new object[] { true });
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void CallExplicitExpansion<TSource, TDestination, TMember>(IMemberConfigurationExpression<TSource, TDestination, TMember> options)
            => options.ExplicitExpansion();

        /// <summary>A resolved value as the member's type: as it is when it already is one, otherwise mapped.</summary>
        public static TMember ToMember<TMember>(object value, ResolutionContext context)
        {
            if (value is TMember member)
                return member;
            if (value == null)
                return default;
            return (TMember)context.Mapper.Map(value, value.GetType(), typeof(TMember));
        }
    }

    /// <summary>
    /// A resolver given to <c>ResolveUsing</c>, with what <c>FromMember</c> and <c>ConstructedBy</c> set afterwards.
    /// It is read when a value is mapped.
    /// </summary>
    internal sealed class ResolverConfiguration<TSource> : IResolverConfigurationExpression, IResolverConfigurationExpression<TSource>
    {
        private readonly Type _resolverType;
        private IValueResolver _resolver;
        private Func<IValueResolver> _constructor;
        private Func<TSource, object> _sourceMember;

        public ResolverConfiguration(Type resolverType) => _resolverType = resolverType;

        public ResolverConfiguration(IValueResolver resolver) => _resolver = resolver;

        /// <summary>The type of the value the resolver receives, when it isn't null.</summary>
        public Type SourceType { get; private set; } = typeof(TSource);

        public object GetSource(TSource source) => _sourceMember != null ? _sourceMember(source) : source;

        public IValueResolver CreateResolver()
        {
            if (_resolver != null)
                return _resolver;
            if (_constructor != null)
                return _constructor();
            // As 3.x did for each value: the service constructor (ConstructServicesUsing), or the default constructor.
            return (IValueResolver)Mapper.ConstructService(_resolverType);
        }

        public void SetConstructor(Func<IValueResolver> constructor) => _constructor = constructor;

        public void FromMember(Expression<Func<TSource, object>> sourceMember)
        {
            var compiled = sourceMember.Compile();
            SourceType = GetMemberType(sourceMember.Body) ?? typeof(object);
            _sourceMember = source => source == null ? null : compiled(source);
        }

        public void FromMember(string sourcePropertyName)
        {
            // The application's own type's member, by name, as 3.x looked it up: on the source type, or, for a map
            // between types given at runtime (CreateMap(Type, Type), whose TSource is object), on the source object's type.
            var member = FindMember(typeof(TSource), sourcePropertyName);
            if (member != null)
                SourceType = member is PropertyInfo p ? p.PropertyType : ((FieldInfo)member).FieldType;
            _sourceMember = source =>
            {
                if (source == null)
                    return null;
                var found = member ?? FindMember(source.GetType(), sourcePropertyName) ??
                    throw new ArgumentOutOfRangeException(nameof(sourcePropertyName),
                        $"{source.GetType()} has no property or field {sourcePropertyName}.");
                return found is PropertyInfo property ? property.GetValue(source) : ((FieldInfo)found).GetValue(source);
            };
        }

        private static MemberInfo FindMember(Type type, string name)
            => (MemberInfo)type.GetProperty(name) ?? type.GetField(name);

        IResolutionExpression IResolverConfigurationExpression.ConstructedBy(Func<IValueResolver> constructor)
        {
            _constructor = constructor;
            return this;
        }

        IResolutionExpression<TSource> IResolverConfigurationExpression<TSource>.ConstructedBy(Func<IValueResolver> constructor)
        {
            _constructor = constructor;
            return this;
        }

        private static Type GetMemberType(Expression body)
        {
            while (body is UnaryExpression unary && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                body = unary.Operand;
            return body.Type;
        }
    }

    internal sealed class ResolverConfiguration<TSource, TValueResolver> : IResolverConfigurationExpression<TSource, TValueResolver>
        where TValueResolver : IValueResolver
    {
        private readonly ResolverConfiguration<TSource> _resolver;

        public ResolverConfiguration(ResolverConfiguration<TSource> resolver) => _resolver = resolver;

        public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(Expression<Func<TSource, object>> sourceMember)
        {
            _resolver.FromMember(sourceMember);
            return this;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> FromMember(string sourcePropertyName)
        {
            _resolver.FromMember(sourcePropertyName);
            return this;
        }

        public IResolverConfigurationExpression<TSource, TValueResolver> ConstructedBy(Func<TValueResolver> constructor)
        {
            _resolver.SetConstructor(() => constructor());
            return this;
        }
    }
}
