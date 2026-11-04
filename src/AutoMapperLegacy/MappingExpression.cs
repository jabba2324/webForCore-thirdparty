// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 and 4.2.1 (IMappingExpression), Copyright (c)
// 2010 Jimmy Bogard, MIT License (see NOTICE).
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Configuration;
using AutoMapper.Features;

namespace AutoMapperLegacy
{
    /// <summary>
    /// The map <c>CreateMap&lt;TSource, TDestination&gt;()</c> returns: AutoMapper 3.x's members, and AutoMapper 12's
    /// <see cref="IMappingExpression{TSource,TDestination}"/>, which it implements.
    /// </summary>
    /// <remarks>
    /// It records each call and replays them on AutoMapper 12's expression each time the facade builds a configuration,
    /// so maps can be changed until they are used, as in 3.x. Its own <c>ConvertUsing</c> and <c>ConstructUsing</c>
    /// take functions, as 3.x's did, so statement lambdas compile: AutoMapper 12's take expression trees (they are
    /// still there through the interface). The lambdas given to <c>ForMember</c> run again at each build.
    /// </remarks>
    public sealed class LegacyMappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>, IRecordedMap
    {
        private readonly List<Action<IMappingExpression<TSource, TDestination>>> _actions = new();
        private readonly List<ValueTransformerConfiguration> _valueTransformers = new();
        private bool _hasReverseMap;

        bool IRecordedMap.HasReverseMap => _hasReverseMap;

        internal LegacyMappingExpression()
        {
        }

        private LegacyMappingExpression<TSource, TDestination> Record(Action<IMappingExpression<TSource, TDestination>> action)
        {
            lock (Mapper.Sync)
            {
                _actions.Add(action);
                Mapper.Invalidate();
            }
            return this;
        }

        internal void Replay(IMappingExpression<TSource, TDestination> expression)
        {
            // 3.x created a proxy for an interface destination; AutoMapper 12 does when told to.
            if (typeof(TDestination).IsInterface)
                expression.AsProxy();
            // 3.x's [IgnoreMap] on the destination type's members; the map's own configuration comes after it.
            foreach (var member in IgnoreMapAttribute.IgnoredMembers(typeof(TSource), typeof(TDestination)))
                expression.ForMember(member, o => o.Ignore());
            foreach (var action in _actions.ToArray())
                action(expression);
            if (_valueTransformers.Count > 0)
                expression.ValueTransformers.AddRange(_valueTransformers);
        }

        // AutoMapper 3.x and 4.x

        public LegacyMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
            Action<LegacyMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
            => Record(e => e.ForMember(destinationMember, o => memberOptions(new LegacyMemberConfigurationExpression<TSource, TDestination, TMember>(o))));

        public LegacyMappingExpression<TSource, TDestination> ForMember(string name,
            Action<LegacyMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
            => Record(e => e.ForMember(name, o => memberOptions(new LegacyMemberConfigurationExpression<TSource, TDestination, object>(o))));

        public void ForAllMembers(Action<LegacyMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
            => Record(e => e.ForAllMembers(o => memberOptions(new LegacyMemberConfigurationExpression<TSource, TDestination, object>(o))));

        public LegacyMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember,
            Action<ISourceMemberConfigurationExpression> memberOptions)
            => Record(e => e.ForSourceMember(sourceMember, memberOptions));

        public LegacyMappingExpression<TSource, TDestination> ForSourceMember(string sourceMemberName,
            Action<ISourceMemberConfigurationExpression> memberOptions)
            => Record(e => e.ForSourceMember(sourceMemberName, memberOptions));

        public LegacyMappingExpression<TSource, TDestination> IgnoreAllPropertiesWithAnInaccessibleSetter()
            => Record(e => e.IgnoreAllPropertiesWithAnInaccessibleSetter());

        public LegacyMappingExpression<TSource, TDestination> IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            => Record(e => e.IgnoreAllSourcePropertiesWithAnInaccessibleSetter());

        public LegacyMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>()
            where TOtherSource : TSource where TOtherDestination : TDestination
            => Record(e => e.Include<TOtherSource, TOtherDestination>());

        public LegacyMappingExpression<TSource, TDestination> Include(Type derivedSourceType, Type derivedDestinationType)
            => Record(e => e.Include(derivedSourceType, derivedDestinationType));

        public LegacyMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>()
            => Record(e => e.IncludeBase<TSourceBase, TDestinationBase>());

        public LegacyMappingExpression<TSource, TDestination> IncludeBase(Type sourceBase, Type destinationBase)
            => Record(e => e.IncludeBase(sourceBase, destinationBase));

        /// <summary>3.x moved a map into a named profile. Not supported: create the map in the profile.</summary>
        public LegacyMappingExpression<TSource, TDestination> WithProfile(string profileName)
            => throw new NotSupportedException("WithProfile isn't supported by AutoMapperLegacy: call CreateMap on the profile (Mapper.CreateProfile or a Profile's Configure).");

        /// <summary>3.x's projection: AutoMapper 12's <c>ConvertUsing(Expression)</c>, which also maps.</summary>
        public void ProjectUsing(Expression<Func<TSource, TDestination>> projectionExpression)
            => Record(e => e.ConvertUsing(projectionExpression));

        public void ConvertUsing(Func<TSource, TDestination> mappingFunction)
            => Record(e => e.ConvertUsing((source, destination) => mappingFunction(source)));

        public void ConvertUsing(ITypeConverter<TSource, TDestination> converter)
            => Record(e => e.ConvertUsing(Converters.Adapt(converter)));

        public void ConvertUsing(AutoMapper.ITypeConverter<TSource, TDestination> converter)
            => Record(e => e.ConvertUsing(converter));

        /// <summary>A 3.x converter type (<see cref="ITypeConverter{TSource,TDestination}"/>) or an AutoMapper 12 one.</summary>
        public void ConvertUsing<TTypeConverter>() => ConvertUsing(typeof(TTypeConverter));

        public void ConvertUsing(Type typeConverterType)
        {
            if (typeof(ITypeConverter<TSource, TDestination>).IsAssignableFrom(typeConverterType))
                Record(e => e.ConvertUsing(Converters.Adapt<TSource, TDestination>(typeConverterType)));
            else
                Record(e => e.ConvertUsing(typeConverterType));
        }

        /// <summary>
        /// 4.x's <c>ConvertUsing(Func&lt;ResolutionContext, TDestination&gt;)</c>, which
        /// <see cref="LegacyMappingExpressionExtensions"/> provides as an extension method: as an overload here it would
        /// make 3.x's <c>ConvertUsing(x =&gt; Convert.ToInt32(x))</c> ambiguous.
        /// </summary>
        internal void ConvertUsingContext(Func<ResolutionContext, TDestination> mappingFunction)
            => Record(e => e.ConvertUsing((source, destination, context) =>
                LegacyFrame.Run(context, source, typeof(TSource), destination, typeof(TDestination), null, () => mappingFunction(context))));

        /// <summary>4.x's conversion over the context and the source.</summary>
        public void ConvertUsing(Func<ResolutionContext, TSource, TDestination> mappingFunction)
            => Record(e => e.ConvertUsing((source, destination, context) =>
                LegacyFrame.Run(context, source, typeof(TSource), destination, typeof(TDestination), null, () => mappingFunction(context, source))));

        /// <summary>AutoMapper 12's conversion. Its two-argument form is only on the interface, as it would make 3.x's
        /// <c>ConvertUsing(int.Parse)</c> ambiguous.</summary>
        public void ConvertUsing(Func<TSource, TDestination, ResolutionContext, TDestination> mappingFunction)
            => Record(e => e.ConvertUsing(mappingFunction));

        public LegacyMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
            => Record(e => e.BeforeMap(beforeFunction));

        public LegacyMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction)
            => Record(e => e.BeforeMap(beforeFunction));

        /// <summary>A 3.x action type (<see cref="IMappingAction{TSource,TDestination}"/>) or an AutoMapper 12 one.</summary>
        public LegacyMappingExpression<TSource, TDestination> BeforeMap<TMappingAction>()
            => Record(e => e.BeforeMap(Converters.Action<TSource, TDestination>(typeof(TMappingAction))));

        public LegacyMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
            => Record(e => e.AfterMap(afterFunction));

        public LegacyMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction)
            => Record(e => e.AfterMap(afterFunction));

        /// <summary>A 3.x action type (<see cref="IMappingAction{TSource,TDestination}"/>) or an AutoMapper 12 one.</summary>
        public LegacyMappingExpression<TSource, TDestination> AfterMap<TMappingAction>()
            => Record(e => e.AfterMap(Converters.Action<TSource, TDestination>(typeof(TMappingAction))));

        public LegacyMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor)
            => Record(e => e.ConstructUsing((source, context) => ctor(source)));

        /// <summary>
        /// 3.x's <c>ConstructUsing(Func&lt;ResolutionContext, TDestination&gt;)</c>, which
        /// <see cref="LegacyMappingExpressionExtensions"/> provides as an extension method, so that a lambda valid for
        /// both (<c>x =&gt; new Foo()</c>, ambiguous in 3.x) takes the source.
        /// </summary>
        internal LegacyMappingExpression<TSource, TDestination> ConstructUsingContext(Func<ResolutionContext, TDestination> ctor)
            => Record(e => e.ConstructUsing((source, context) =>
                LegacyFrame.Run(context, source, typeof(TSource), null, typeof(TDestination), null, () => ctor(context))));

        public LegacyMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, ResolutionContext, TDestination> ctor)
            => Record(e => e.ConstructUsing(ctor));

        public LegacyMappingExpression<TSource, TDestination> ConstructProjectionUsing(Expression<Func<TSource, TDestination>> ctor)
            => Record(e => e.ConstructUsing(ctor));

        public void As<T>() where T : TDestination => Record(e => e.As<T>());

        public void As(Type typeOverride) => Record(e => e.As(typeOverride));

        public LegacyMappingExpression<TSource, TDestination> MaxDepth(int depth) => Record(e => e.MaxDepth(depth));

        public LegacyMappingExpression<TSource, TDestination> ConstructUsingServiceLocator() => Record(e => e.ConstructUsingServiceLocator());

        public LegacyMappingExpression<TDestination, TSource> ReverseMap()
        {
            var reverse = new LegacyMappingExpression<TDestination, TSource>();
            _hasReverseMap = true;
            Record(e => reverse.Replay(e.ReverseMap()));
            return reverse;
        }

        /// <summary>3.x's <c>Substitute</c>: maps the object the function returns instead of the source.</summary>
        public LegacyMappingExpression<TSource, TDestination> Substitute(Func<TSource, object> substituteFunc)
            => Record(e => e.ConvertUsing((source, destination, context) =>
            {
                var substitute = substituteFunc(source);
                return substitute == null
                    ? destination
                    : (TDestination)context.Mapper.Map(substitute, destination, substitute.GetType(), typeof(TDestination));
            }));

        /// <summary>The map's AutoMapper 12 <see cref="TypeMap"/>, from the current configuration.</summary>
        public TypeMap TypeMap => Mapper.FindTypeMapFor<TSource, TDestination>();

        // AutoMapper 12, for code that already uses it

        public LegacyMappingExpression<TSource, TDestination> ForPath<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
            Action<IPathConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
            => Record(e => e.ForPath(destinationMember, memberOptions));

        public LegacyMappingExpression<TSource, TDestination> IncludeAllDerived() => Record(e => e.IncludeAllDerived());

        public LegacyMappingExpression<TSource, TDestination> IncludeMembers(params Expression<Func<TSource, object>>[] memberExpressions)
            => Record(e => e.IncludeMembers(memberExpressions));

        public LegacyMappingExpression<TSource, TDestination> PreserveReferences() => Record(e => e.PreserveReferences());

        public LegacyMappingExpression<TSource, TDestination> DisableCtorValidation() => Record(e => e.DisableCtorValidation());

        public LegacyMappingExpression<TSource, TDestination> ValidateMemberList(MemberList memberList) => Record(e => e.ValidateMemberList(memberList));

        public LegacyMappingExpression<TSource, TDestination> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
            => Record(e => e.ForCtorParam(ctorParamName, paramOptions));

        public LegacyMappingExpression<TSource, TDestination> AsProxy() => Record(e => e.AsProxy());

        public LegacyMappingExpression<TSource, TDestination> AddTransform<TValue>(Expression<Func<TValue, TValue>> transformer)
            => Record(e => e.AddTransform(transformer));

        // AutoMapper 12's interface, recorded the same way

        Features<IMappingFeature> IProjectionExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.Features
            => throw new NotSupportedException("AutoMapperLegacy doesn't support mapping features.");

        List<ValueTransformerConfiguration> IProjectionExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ValueTransformers
            => _valueTransformers;

        IMappingExpression<TSource, TDestination> IMappingExpression<TSource, TDestination>.ForPath<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IPathConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
            => ForPath(destinationMember, memberOptions);
        IMappingExpression<TSource, TDestination> IMappingExpression<TSource, TDestination>.ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
            => Record(e => e.ForMember(destinationMember, memberOptions));
        IMappingExpression<TSource, TDestination> IMappingExpression<TSource, TDestination>.ForMember(string name, Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
            => Record(e => e.ForMember(name, memberOptions));
        void IMappingExpression<TSource, TDestination>.ForAllMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
            => Record(e => e.ForAllMembers(memberOptions));
        IMappingExpression<TSource, TDestination> IMappingExpression<TSource, TDestination>.Include<TOtherSource, TOtherDestination>() => Include<TOtherSource, TOtherDestination>();
        IMappingExpression<TSource, TDestination> IMappingExpression<TSource, TDestination>.IncludeBase<TSourceBase, TDestinationBase>() => IncludeBase<TSourceBase, TDestinationBase>();
        IMappingExpression<TSource, TDestination> IMappingExpression<TSource, TDestination>.ForSourceMember(Expression<Func<TSource, object>> sourceMember, Action<ISourceMemberConfigurationExpression> memberOptions)
            => ForSourceMember(sourceMember, memberOptions);
        IMappingExpression<TDestination, TSource> IMappingExpression<TSource, TDestination>.ReverseMap() => ReverseMap();
        void IMappingExpression<TSource, TDestination>.As<T>() => As<T>();
        IMappingExpression<TSource, TDestination> IProjectionExpression<TSource, TDestination, IMappingExpression<TSource, TDestination>>.AddTransform<TValue>(Expression<Func<TValue, TValue>> transformer) => AddTransform(transformer);
        IMappingExpression<TSource, TDestination> IProjectionExpression<TSource, TDestination, IMappingExpression<TSource, TDestination>>.IncludeMembers(params Expression<Func<TSource, object>>[] memberExpressions) => IncludeMembers(memberExpressions);
        IMappingExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.MaxDepth(int depth) => MaxDepth(depth);
        IMappingExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ValidateMemberList(MemberList memberList) => ValidateMemberList(memberList);
        IMappingExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConstructUsing(Expression<Func<TSource, TDestination>> ctor) => Record(e => e.ConstructUsing(ctor));
        IMappingExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions) => ForCtorParam(ctorParamName, paramOptions);
        void IProjectionExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConvertUsing(Expression<Func<TSource, TDestination>> mappingExpression) => Record(e => e.ConvertUsing(mappingExpression));
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.DisableCtorValidation() => DisableCtorValidation();
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConstructUsingServiceLocator() => ConstructUsingServiceLocator();
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.PreserveReferences() => PreserveReferences();
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.BeforeMap(Action<TSource, TDestination> beforeFunction) => BeforeMap(beforeFunction);
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction) => BeforeMap(beforeFunction);
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.BeforeMap<TMappingAction>() => Record(e => e.BeforeMap<TMappingAction>());
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.AfterMap(Action<TSource, TDestination> afterFunction) => AfterMap(afterFunction);
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction) => AfterMap(afterFunction);
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.AfterMap<TMappingAction>() => Record(e => e.AfterMap<TMappingAction>());
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.IncludeAllDerived() => IncludeAllDerived();
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.Include(Type derivedSourceType, Type derivedDestinationType) => Include(derivedSourceType, derivedDestinationType);
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.IncludeBase(Type sourceBase, Type destinationBase) => IncludeBase(sourceBase, destinationBase);
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions) => ForSourceMember(sourceMemberName, memberOptions);
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.IgnoreAllPropertiesWithAnInaccessibleSetter() => IgnoreAllPropertiesWithAnInaccessibleSetter();
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.IgnoreAllSourcePropertiesWithAnInaccessibleSetter() => IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConstructUsing(Func<TSource, ResolutionContext, TDestination> ctor) => ConstructUsing(ctor);
        void IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.As(Type typeOverride) => As(typeOverride);
        IMappingExpression<TSource, TDestination> IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.AsProxy() => AsProxy();
        void IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConvertUsing(Type typeConverterType) => ConvertUsing(typeConverterType);
        void IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConvertUsing(Func<TSource, TDestination, TDestination> mappingFunction) => Record(e => e.ConvertUsing(mappingFunction));
        void IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConvertUsing(Func<TSource, TDestination, ResolutionContext, TDestination> mappingFunction) => ConvertUsing(mappingFunction);
        void IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConvertUsing(AutoMapper.ITypeConverter<TSource, TDestination> converter) => ConvertUsing(converter);
        void IMappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>.ConvertUsing<TTypeConverter>() => Record(e => e.ConvertUsing<TTypeConverter>());
    }

    /// <summary>
    /// The map <c>CreateMap(Type, Type)</c> returns: AutoMapper 3.x's members, and AutoMapper 12's
    /// <see cref="IMappingExpression"/>, which it implements. It records and replays as
    /// <see cref="LegacyMappingExpression{TSource,TDestination}"/> does.
    /// </summary>
    public sealed class LegacyMappingExpression : IMappingExpression, IRecordedMap
    {
        private readonly List<Action<IMappingExpression>> _actions = new();
        private readonly List<ValueTransformerConfiguration> _valueTransformers = new();
        private bool _hasReverseMap;

        bool IRecordedMap.HasReverseMap => _hasReverseMap;

        private readonly Type _sourceType;
        private readonly Type _destinationType;

        internal LegacyMappingExpression(Type sourceType, Type destinationType)
        {
            _sourceType = sourceType;
            _destinationType = destinationType;
        }

        private LegacyMappingExpression Record(Action<IMappingExpression> action)
        {
            lock (Mapper.Sync)
            {
                _actions.Add(action);
                Mapper.Invalidate();
            }
            return this;
        }

        internal void Replay(IMappingExpression expression)
        {
            if (_destinationType != null && _destinationType.IsInterface)
                expression.AsProxy();
            // 3.x's [IgnoreMap]; the map's own configuration comes after it.
            foreach (var member in IgnoreMapAttribute.IgnoredMembers(_sourceType, _destinationType))
                expression.ForMember(member, o => o.Ignore());
            foreach (var action in _actions.ToArray())
                action(expression);
            if (_valueTransformers.Count > 0)
                expression.ValueTransformers.AddRange(_valueTransformers);
        }

        // AutoMapper 3.x and 4.x

        public LegacyMappingExpression ForMember(string name, Action<LegacyMemberConfigurationExpression> memberOptions)
            => Record(e => e.ForMember(name, o => memberOptions(new LegacyMemberConfigurationExpression(o))));

        public void ForAllMembers(Action<LegacyMemberConfigurationExpression> memberOptions)
            => Record(e => e.ForAllMembers(o => memberOptions(new LegacyMemberConfigurationExpression(o))));

        public LegacyMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
            => Record(e => e.ForSourceMember(sourceMemberName, memberOptions));

        /// <summary>A 3.x converter type (<see cref="ITypeConverter{TSource,TDestination}"/>) or an AutoMapper 12 one.</summary>
        public void ConvertUsing<TTypeConverter>() => ConvertUsing(typeof(TTypeConverter));

        public void ConvertUsing(Type typeConverterType)
            => Record(e => e.ConvertUsing(Converters.AdaptType(typeConverterType, _sourceType, _destinationType)));

        public void As(Type typeOverride) => Record(e => e.As(typeOverride));

        /// <summary>3.x moved a map into a named profile. Not supported: create the map in the profile.</summary>
        public LegacyMappingExpression WithProfile(string profileName)
            => throw new NotSupportedException("WithProfile isn't supported by AutoMapperLegacy: call CreateMap on the profile (Mapper.CreateProfile or a Profile's Configure).");

        public LegacyMappingExpression ReverseMap()
        {
            var reverse = new LegacyMappingExpression(_destinationType, _sourceType);
            _hasReverseMap = true;
            Record(e => reverse.Replay(e.ReverseMap()));
            return reverse;
        }

        public LegacyMappingExpression Include(Type derivedSourceType, Type derivedDestinationType) => Record(e => e.Include(derivedSourceType, derivedDestinationType));
        public LegacyMappingExpression IncludeBase(Type sourceBase, Type destinationBase) => Record(e => e.IncludeBase(sourceBase, destinationBase));
        public LegacyMappingExpression IncludeAllDerived() => Record(e => e.IncludeAllDerived());
        public LegacyMappingExpression IncludeMembers(params string[] memberNames) => Record(e => e.IncludeMembers(memberNames));
        public LegacyMappingExpression BeforeMap(Action<object, object> beforeFunction) => Record(e => e.BeforeMap(beforeFunction));
        public LegacyMappingExpression BeforeMap(Action<object, object, ResolutionContext> beforeFunction) => Record(e => e.BeforeMap(beforeFunction));
        public LegacyMappingExpression AfterMap(Action<object, object> afterFunction) => Record(e => e.AfterMap(afterFunction));
        public LegacyMappingExpression AfterMap(Action<object, object, ResolutionContext> afterFunction) => Record(e => e.AfterMap(afterFunction));
        public LegacyMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter() => Record(e => e.IgnoreAllPropertiesWithAnInaccessibleSetter());
        public LegacyMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter() => Record(e => e.IgnoreAllSourcePropertiesWithAnInaccessibleSetter());
        public LegacyMappingExpression ConstructUsing(Func<object, ResolutionContext, object> ctor) => Record(e => e.ConstructUsing(ctor));
        public LegacyMappingExpression ConstructUsingServiceLocator() => Record(e => e.ConstructUsingServiceLocator());
        public LegacyMappingExpression MaxDepth(int depth) => Record(e => e.MaxDepth(depth));
        public LegacyMappingExpression PreserveReferences() => Record(e => e.PreserveReferences());
        public LegacyMappingExpression DisableCtorValidation() => Record(e => e.DisableCtorValidation());
        public LegacyMappingExpression ValidateMemberList(MemberList memberList) => Record(e => e.ValidateMemberList(memberList));
        public LegacyMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions) => Record(e => e.ForCtorParam(ctorParamName, paramOptions));
        public LegacyMappingExpression AsProxy() => Record(e => e.AsProxy());
        public void ConvertUsing(Func<object, object, object> mappingFunction) => Record(e => e.ConvertUsing(mappingFunction));
        public void ConvertUsing(Func<object, object, ResolutionContext, object> mappingFunction) => Record(e => e.ConvertUsing(mappingFunction));

        // AutoMapper 12's interface, recorded the same way

        Features<IMappingFeature> IProjectionExpressionBase<object, object, IMappingExpression>.Features
            => throw new NotSupportedException("AutoMapperLegacy doesn't support mapping features.");
        List<ValueTransformerConfiguration> IProjectionExpressionBase<object, object, IMappingExpression>.ValueTransformers => _valueTransformers;
        IMappingExpression IMappingExpression.IncludeMembers(params string[] memberNames) => IncludeMembers(memberNames);
        IMappingExpression IMappingExpression.ReverseMap() => ReverseMap();
        void IMappingExpression.ForAllMembers(Action<IMemberConfigurationExpression> memberOptions) => Record(e => e.ForAllMembers(memberOptions));
        IMappingExpression IMappingExpression.ForMember(string name, Action<IMemberConfigurationExpression> memberOptions) => Record(e => e.ForMember(name, memberOptions));
        IMappingExpression IProjectionExpressionBase<object, object, IMappingExpression>.MaxDepth(int depth) => MaxDepth(depth);
        IMappingExpression IProjectionExpressionBase<object, object, IMappingExpression>.ValidateMemberList(MemberList memberList) => ValidateMemberList(memberList);
        IMappingExpression IProjectionExpressionBase<object, object, IMappingExpression>.ConstructUsing(Expression<Func<object, object>> ctor) => Record(e => e.ConstructUsing(ctor));
        IMappingExpression IProjectionExpressionBase<object, object, IMappingExpression>.ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<object>> paramOptions) => ForCtorParam(ctorParamName, paramOptions);
        void IProjectionExpressionBase<object, object, IMappingExpression>.ConvertUsing(Expression<Func<object, object>> mappingExpression) => Record(e => e.ConvertUsing(mappingExpression));
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.DisableCtorValidation() => DisableCtorValidation();
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.ConstructUsingServiceLocator() => ConstructUsingServiceLocator();
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.PreserveReferences() => PreserveReferences();
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.BeforeMap(Action<object, object> beforeFunction) => BeforeMap(beforeFunction);
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.BeforeMap(Action<object, object, ResolutionContext> beforeFunction) => BeforeMap(beforeFunction);
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.BeforeMap<TMappingAction>() => Record(e => e.BeforeMap<TMappingAction>());
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.AfterMap(Action<object, object> afterFunction) => AfterMap(afterFunction);
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.AfterMap(Action<object, object, ResolutionContext> afterFunction) => AfterMap(afterFunction);
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.AfterMap<TMappingAction>() => Record(e => e.AfterMap<TMappingAction>());
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.IncludeAllDerived() => IncludeAllDerived();
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.Include(Type derivedSourceType, Type derivedDestinationType) => Include(derivedSourceType, derivedDestinationType);
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.IncludeBase(Type sourceBase, Type destinationBase) => IncludeBase(sourceBase, destinationBase);
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions) => ForSourceMember(sourceMemberName, memberOptions);
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.IgnoreAllPropertiesWithAnInaccessibleSetter() => IgnoreAllPropertiesWithAnInaccessibleSetter();
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.IgnoreAllSourcePropertiesWithAnInaccessibleSetter() => IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.ConstructUsing(Func<object, ResolutionContext, object> ctor) => ConstructUsing(ctor);
        void IMappingExpressionBase<object, object, IMappingExpression>.As(Type typeOverride) => As(typeOverride);
        IMappingExpression IMappingExpressionBase<object, object, IMappingExpression>.AsProxy() => AsProxy();
        void IMappingExpressionBase<object, object, IMappingExpression>.ConvertUsing(Type typeConverterType) => ConvertUsing(typeConverterType);
        void IMappingExpressionBase<object, object, IMappingExpression>.ConvertUsing(Func<object, object, object> mappingFunction) => ConvertUsing(mappingFunction);
        void IMappingExpressionBase<object, object, IMappingExpression>.ConvertUsing(Func<object, object, ResolutionContext, object> mappingFunction) => ConvertUsing(mappingFunction);
        void IMappingExpressionBase<object, object, IMappingExpression>.ConvertUsing(AutoMapper.ITypeConverter<object, object> converter) => Record(e => e.ConvertUsing(converter));
        void IMappingExpressionBase<object, object, IMappingExpression>.ConvertUsing<TTypeConverter>() => Record(e => e.ConvertUsing<TTypeConverter>());
    }
}

namespace AutoMapperLegacy
{
    /// <summary>What the facade reads from a recorded map when it builds a configuration.</summary>
    internal interface IRecordedMap
    {
        /// <summary>Whether <c>ReverseMap</c> was called, so the map the other way exists too.</summary>
        bool HasReverseMap { get; }
    }
}
