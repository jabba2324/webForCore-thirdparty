// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 and 4.2.1, Copyright (c) 2010 Jimmy Bogard
// (MIT License; see NOTICE).
using System;
using AutoMapper;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x's mapping engine: <see cref="Mapper.Engine"/>, and <c>ResolutionContext.Engine</c> inside a
    /// converter or resolver.
    /// </summary>
    public interface IMappingEngine : IDisposable
    {
        /// <summary>AutoMapper 12's configuration the engine maps with.</summary>
        IConfigurationProvider ConfigurationProvider { get; }

        TDestination Map<TDestination>(object source);
        TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts);
        TDestination Map<TSource, TDestination>(TSource source);
        TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts);
        object Map(object source, Type sourceType, Type destinationType);
        object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts);
        object Map(object source, object destination, Type sourceType, Type destinationType);
        object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts);
        TDestination DynamicMap<TSource, TDestination>(TSource source);
        TDestination DynamicMap<TDestination>(object source);
        object DynamicMap(object source, Type sourceType, Type destinationType);
        void DynamicMap<TSource, TDestination>(TSource source, TDestination destination);
        void DynamicMap(object source, object destination, Type sourceType, Type destinationType);
    }

    /// <summary><see cref="Mapper.Engine"/>: each call goes to the static <see cref="Mapper"/>'s current configuration.</summary>
    internal sealed class StaticMappingEngine : IMappingEngine
    {
        public static readonly StaticMappingEngine Instance = new StaticMappingEngine();

        public IConfigurationProvider ConfigurationProvider => Mapper.ConfigurationProvider;
        public TDestination Map<TDestination>(object source) => Mapper.Map<TDestination>(source);
        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts) => Mapper.Map<TDestination>(source, opts);
        public TDestination Map<TSource, TDestination>(TSource source) => Mapper.Map<TSource, TDestination>(source);
        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts) => Mapper.Map(source, opts);
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) => Mapper.Map(source, destination);
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts) => Mapper.Map(source, destination, opts);
        public object Map(object source, Type sourceType, Type destinationType) => Mapper.Map(source, sourceType, destinationType);
        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts) => Mapper.Map(source, sourceType, destinationType, opts);
        public object Map(object source, object destination, Type sourceType, Type destinationType) => Mapper.Map(source, destination, sourceType, destinationType);
        public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts) => Mapper.Map(source, destination, sourceType, destinationType, opts);
        public TDestination DynamicMap<TSource, TDestination>(TSource source) => Mapper.DynamicMap<TSource, TDestination>(source);
        public TDestination DynamicMap<TDestination>(object source) => Mapper.DynamicMap<TDestination>(source);
        public object DynamicMap(object source, Type sourceType, Type destinationType) => Mapper.DynamicMap(source, sourceType, destinationType);
        public void DynamicMap<TSource, TDestination>(TSource source, TDestination destination) => Mapper.DynamicMap(source, destination);
        public void DynamicMap(object source, object destination, Type sourceType, Type destinationType) => Mapper.DynamicMap(source, destination, sourceType, destinationType);
        public void Dispose() { }
    }

    /// <summary>
    /// <c>ResolutionContext.Engine</c>: maps within the current operation, with AutoMapper 12's runtime mapper, so a
    /// nested map sees the same <c>Options.Items</c>. The options of a nested call aren't applied (12's runtime mapper
    /// has no options), and <c>DynamicMap</c> creates missing maps in the static <see cref="Mapper"/> and maps there.
    /// </summary>
    internal sealed class RuntimeMappingEngine : IMappingEngine
    {
        private readonly IRuntimeMapper _mapper;

        public RuntimeMappingEngine(IRuntimeMapper mapper) => _mapper = mapper;

        public IConfigurationProvider ConfigurationProvider => Mapper.ConfigurationProvider;
        public TDestination Map<TDestination>(object source) => _mapper.Map<TDestination>(source);
        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts) => _mapper.Map<TDestination>(source);
        public TDestination Map<TSource, TDestination>(TSource source) => _mapper.Map<TSource, TDestination>(source);
        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts) => _mapper.Map<TSource, TDestination>(source);
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) => _mapper.Map(source, destination);
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts) => _mapper.Map(source, destination);
        public object Map(object source, Type sourceType, Type destinationType) => _mapper.Map(source, sourceType, destinationType);
        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts) => _mapper.Map(source, sourceType, destinationType);
        public object Map(object source, object destination, Type sourceType, Type destinationType) => _mapper.Map(source, destination, sourceType, destinationType);
        public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts) => _mapper.Map(source, destination, sourceType, destinationType);
        public TDestination DynamicMap<TSource, TDestination>(TSource source) => Mapper.DynamicMap<TSource, TDestination>(source);
        public TDestination DynamicMap<TDestination>(object source) => Mapper.DynamicMap<TDestination>(source);
        public object DynamicMap(object source, Type sourceType, Type destinationType) => Mapper.DynamicMap(source, sourceType, destinationType);
        public void DynamicMap<TSource, TDestination>(TSource source, TDestination destination) => Mapper.DynamicMap(source, destination);
        public void DynamicMap(object source, object destination, Type sourceType, Type destinationType) => Mapper.DynamicMap(source, destination, sourceType, destinationType);
        public void Dispose() { }
    }
}
