// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 and 4.2.1 (Mapper), Copyright (c) 2010 Jimmy
// Bogard, MIT License (see NOTICE).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using AutoMapper;
using AutoMapper.Internal;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x and 4.x's static <c>Mapper</c>, on AutoMapper 12. AutoMapper 12 has an instance class
    /// <c>AutoMapper.Mapper</c>, so an application adds <c>global using Mapper = AutoMapperLegacy.Mapper;</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Maps, profiles and settings are recorded as they're registered (<see cref="Initialize"/>, <see cref="CreateMap{TSource,TDestination}()"/>,
    /// <see cref="AddProfile(Profile)"/>, ...), at any time. The first <c>Map</c> after a registration builds an
    /// AutoMapper 12 <see cref="MapperConfiguration"/> and <see cref="IMapper"/> from everything registered, as 3.x
    /// let maps be added at any time before they're used. Building takes a lock, as do registrations; mapping doesn't.
    /// </para>
    /// <para>
    /// Every <c>Map</c> call passes options to AutoMapper 12, so that <c>context.Options.Items</c> works in converters
    /// and resolvers as in 3.x.
    /// </para>
    /// </remarks>
    public static class Mapper
    {
        internal static readonly object Sync = new();

        private static RootConfiguration _root = new();
        private static MapperConfiguration _configuration;
        private static IMapper _mapper;
        private static HashSet<Type> _assignedTypes = new();

        /// <summary>Discards the built configuration; the next use builds it again from what's registered.</summary>
        internal static void Invalidate()
        {
            lock (Sync)
            {
                _configuration = null;
                _mapper = null;
                _assignedTypes = new HashSet<Type>();
            }
        }

        /// <summary>
        /// Whether the map is one the facade added (a type to itself, returning the value, as 3.x assigned it: issue #11),
        /// which 3.x didn't have, so <see cref="FindTypeMapFor(Type, Type)"/> and <see cref="GetAllTypeMaps"/> leave it out.
        /// </summary>
        private static bool IsAssignedTypeMap(TypeMap typeMap)
        {
            lock (Sync)
                return typeMap.SourceType == typeMap.DestinationType && _assignedTypes.Contains(typeMap.DestinationType);
        }

        private static IMapper Current
        {
            get
            {
                lock (Sync)
                {
                    if (_mapper == null)
                    {
                        var configuration = _root.Build(out var assignedTypes);
                        _mapper = configuration.CreateMapper();
                        _configuration = configuration;
                        _assignedTypes = new HashSet<Type>(assignedTypes);
                    }
                    return _mapper;
                }
            }
        }

        /// <summary>Constructs a resolver, converter or action, with <c>ConstructServicesUsing</c>'s function if there is one.</summary>
        internal static object ConstructService(Type type)
        {
            Func<Type, object> serviceCtor = _operationServiceCtor;
            if (serviceCtor == null)
            {
                lock (Sync)
                    serviceCtor = _root.ServiceCtor;
            }
            // The application's own type, constructed as 3.x did when no service constructor is configured.
            return serviceCtor != null ? serviceCtor(type) : Activator.CreateInstance(type, nonPublic: true);
        }

        /// <summary>The configuration: maps, profiles and settings (3.x's <c>IConfiguration</c>; 4.x called it <c>IMapperConfiguration</c>, a name this package doesn't use because applications have their own, Umbraco 7 among them).</summary>
        public static IConfiguration Configuration
        {
            get
            {
                lock (Sync)
                    return _root;
            }
        }

        /// <summary>AutoMapper 12's configuration, built from what's registered.</summary>
        public static IConfigurationProvider ConfigurationProvider
        {
            get
            {
                lock (Sync)
                {
                    _ = Current;
                    return _configuration;
                }
            }
        }

        /// <summary>4.x: AutoMapper 12's mapper, built from what's registered.</summary>
        public static IMapper Instance => Current;

        /// <summary>3.x's mapping engine; each call maps with the current configuration.</summary>
        public static IMappingEngine Engine => StaticMappingEngine.Instance;

        /// <summary>Whether a null source member gives a null destination member (default true).</summary>
        public static bool AllowNullDestinationValues
        {
            get => Configuration.AllowNullDestinationValues;
            set => Configuration.AllowNullDestinationValues = value;
        }

        /// <summary>
        /// Clears the configuration and runs <paramref name="action"/> on a new one. The maps are built on first use,
        /// not here (3.x compiled them here).
        /// </summary>
        public static void Initialize(Action<IConfiguration> action)
        {
            lock (Sync)
            {
                Reset();
                action(_root);
            }
        }

        /// <summary>Clears all configuration.</summary>
        public static void Reset()
        {
            lock (Sync)
            {
                _root = new RootConfiguration();
                Invalidate();
            }
        }

        public static LegacyMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>() => Configuration.CreateMap<TSource, TDestination>();

        public static LegacyMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList) => Configuration.CreateMap<TSource, TDestination>(memberList);

        public static LegacyMappingExpression CreateMap(Type sourceType, Type destinationType) => Configuration.CreateMap(sourceType, destinationType);

        public static LegacyMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList) => Configuration.CreateMap(sourceType, destinationType, memberList);

        public static Profile CreateProfile(string profileName) => Configuration.CreateProfile(profileName);

        public static void CreateProfile(string profileName, Action<Profile> profileConfiguration) => Configuration.CreateProfile(profileName, profileConfiguration);

        public static void AddProfile(Profile profile) => Configuration.AddProfile(profile);

        public static void AddProfile(AutoMapper.Profile profile) => Configuration.AddProfile(profile);

        public static void AddProfile<TProfile>() where TProfile : new() => Configuration.AddProfile<TProfile>();

        public static void AddGlobalIgnore(string startingwith) => Configuration.AddGlobalIgnore(startingwith);

        public static TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
        {
            var typeMap = ConfigurationProvider.Internal().FindTypeMapFor(sourceType, destinationType);
            return typeMap != null && IsAssignedTypeMap(typeMap) ? null : typeMap;
        }

        public static TypeMap FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(typeof(TSource), typeof(TDestination));

        public static TypeMap[] GetAllTypeMaps() => ConfigurationProvider.Internal().GetAllTypeMaps().Where(typeMap => !IsAssignedTypeMap(typeMap)).ToArray();

        /// <summary>AutoMapper 12's validation, which is stricter than 3.x's in places (see the README).</summary>
        public static void AssertConfigurationIsValid() => ConfigurationProvider.AssertConfigurationIsValid();

        public static void AssertConfigurationIsValid(TypeMap typeMap) => ConfigurationProvider.Internal().AssertConfigurationIsValid(typeMap);

        public static void AssertConfigurationIsValid(string profileName) => ConfigurationProvider.Internal().AssertConfigurationIsValid(profileName);

        public static void AssertConfigurationIsValid<TProfile>() where TProfile : Profile, new() => AssertConfigurationIsValid(new TProfile().ProfileName);

        // Mapping

        public static TDestination Map<TDestination>(object source)
        {
            Prepare(source?.GetType(), typeof(TDestination));
            return Current.Map<TDestination>(source, _ => { });
        }

        public static TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            Prepare(source?.GetType(), typeof(TDestination));
            var previous = _operationServiceCtor;
            try { return Current.Map<TDestination>(source, Capture<object, TDestination>(o => opts(o))); }
            finally { _operationServiceCtor = previous; }
        }

        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            Prepare(typeof(TSource), typeof(TDestination));
            return Current.Map<TSource, TDestination>(source, _ => { });
        }

        public static TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Prepare(typeof(TSource), typeof(TDestination));
            var previous = _operationServiceCtor;
            try { return Current.Map<TSource, TDestination>(source, Capture(opts)); }
            finally { _operationServiceCtor = previous; }
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            Prepare(typeof(TSource), typeof(TDestination));
            return Current.Map(source, destination, _ => { });
        }

        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Prepare(typeof(TSource), typeof(TDestination));
            var previous = _operationServiceCtor;
            try { return Current.Map<TSource, TDestination>(source, destination, Capture(opts)); }
            finally { _operationServiceCtor = previous; }
        }

        public static object Map(object source, Type sourceType, Type destinationType)
        {
            Prepare(sourceType, destinationType);
            return Current.Map(source, sourceType, destinationType, _ => { });
        }

        public static object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            Prepare(sourceType, destinationType);
            var previous = _operationServiceCtor;
            try { return Current.Map(source, sourceType, destinationType, Capture<object, object>(o => opts(o))); }
            finally { _operationServiceCtor = previous; }
        }

        public static object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            Prepare(sourceType, destinationType);
            return Current.Map(source, destination, sourceType, destinationType, _ => { });
        }

        public static object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            Prepare(sourceType, destinationType);
            var previous = _operationServiceCtor;
            try { return Current.Map(source, destination, sourceType, destinationType, Capture<object, object>(o => opts(o))); }
            finally { _operationServiceCtor = previous; }
        }

        [ThreadStatic] private static Func<Type, object> _operationServiceCtor;

        /// <summary>
        /// Runs the <c>Map</c> call's options, and if they set <c>ConstructServicesUsing</c>, uses it for the 3.x
        /// resolvers and converters of this operation (on this thread, until the call returns), as 3.x did.
        /// </summary>
        private static Action<IMappingOperationOptions<TSource, TDestination>> Capture<TSource, TDestination>(Action<IMappingOperationOptions<TSource, TDestination>> opts)
            => options =>
            {
                var concrete = options as MappingOperationOptions<TSource, TDestination>;
                var before = concrete?.ServiceCtor;
                opts(options);
                if (concrete != null && !ReferenceEquals(before, concrete.ServiceCtor))
                    _operationServiceCtor = concrete.ServiceCtor;
            };

        // 3.x's DynamicMap: creates the map between the two types if there isn't one, then maps.

        public static TDestination DynamicMap<TSource, TDestination>(TSource source)
        {
            EnsureMap(typeof(TSource), typeof(TDestination));
            return Map<TSource, TDestination>(source);
        }

        public static void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
        {
            EnsureMap(typeof(TSource), typeof(TDestination));
            Map(source, destination);
        }

        public static TDestination DynamicMap<TDestination>(object source)
        {
            EnsureMap(source?.GetType() ?? typeof(object), typeof(TDestination));
            return Map<TDestination>(source);
        }

        public static object DynamicMap(object source, Type sourceType, Type destinationType)
        {
            EnsureMap(sourceType, destinationType);
            return Map(source, sourceType, destinationType);
        }

        public static void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
        {
            EnsureMap(sourceType, destinationType);
            Map(source, destination, sourceType, destinationType);
        }

        /// <summary>4.x's <c>CreateMissingTypeMaps</c>: a missing map between two classes is created.</summary>
        private static void Prepare(Type sourceType, Type destinationType)
        {
            bool create;
            lock (Sync)
                create = _root.CreateMissingTypeMaps;
            if (create && sourceType != null && IsMappableClass(sourceType) && IsMappableClass(destinationType))
                EnsureMap(sourceType, destinationType);
        }

        /// <summary>An application's class (not .NET's own, a delegate or a collection), which needs a map to be mapped.</summary>
        private static bool IsMappableClass(Type type)
            => type.IsClass && type.Assembly != typeof(object).Assembly && !typeof(Delegate).IsAssignableFrom(type) &&
               !typeof(IEnumerable).IsAssignableFrom(type) && !type.IsGenericTypeDefinition;

        /// <summary>
        /// Creates the map between the two types if there isn't one, and, as 3.x's dynamic maps did, the maps between the
        /// types of their members of the same name (and of the elements of collections of the same name) that are an
        /// application's classes without one.
        /// </summary>
        private static void EnsureMap(Type sourceType, Type destinationType)
        {
            lock (Sync)
            {
                var configuration = ConfigurationProvider.Internal();
                var assignedTypes = _assignedTypes;
                var pending = new Stack<(Type Source, Type Destination)>();
                var seen = new HashSet<(Type, Type)>();
                pending.Push((sourceType, destinationType));
                while (pending.Count > 0)
                {
                    var (source, destination) = pending.Pop();
                    // A map the facade added for a type to itself (issue #11) isn't one: 3.x's dynamic map copied the members.
                    if (!seen.Add((source, destination)) || _root.HasMap(source, destination) ||
                        (configuration.FindTypeMapFor(source, destination) != null && !(source == destination && assignedTypes.Contains(source))))
                        continue;
                    _root.CreateMap(source, destination);
                    foreach (var pair in MemberTypePairs(source, destination))
                        pending.Push(pair);
                }
            }
        }

        /// <summary>The members of the same name whose types would need a map (the application's own types).</summary>
        private static IEnumerable<(Type, Type)> MemberTypePairs(Type sourceType, Type destinationType)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            foreach (var destinationProperty in destinationType.GetProperties(flags))
            {
                var sourceProperty = sourceType.GetProperty(destinationProperty.Name, flags);
                if (sourceProperty == null)
                    continue;
                var source = sourceProperty.PropertyType;
                var destination = destinationProperty.PropertyType;
                if (IsCollection(source) && IsCollection(destination))
                {
                    source = ElementType(source);
                    destination = ElementType(destination);
                }
                if (source != null && destination != null && source != destination && IsMappableClass(source) && IsMappableClass(destination))
                    yield return (source, destination);
            }
        }

        private static bool IsCollection(Type type) => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);

        private static Type ElementType(Type collectionType)
            => collectionType.IsArray
                ? collectionType.GetElementType()
                : collectionType.GetInterfaces().Append(collectionType)
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    ?.GetGenericArguments()[0];
    }
}
