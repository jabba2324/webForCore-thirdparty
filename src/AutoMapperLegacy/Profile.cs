// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 and 4.2.1 (Profile, IProfileExpression),
// Copyright (c) 2010 Jimmy Bogard, MIT License (see NOTICE).
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using AutoMapper.Internal;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x's profile: override <see cref="Configure"/> and call <c>CreateMap</c> there (or, as 4.x allowed,
    /// in the constructor). AutoMapper 12 has its own <c>AutoMapper.Profile</c>, so an application with 3.x profiles
    /// adds <c>global using Profile = AutoMapperLegacy.Profile;</c> next to the <c>Mapper</c> alias.
    /// </summary>
    /// <remarks>
    /// The profile records what it's told and the facade replays it into an AutoMapper 12 profile of the same name
    /// each time it builds a configuration. <see cref="Configure"/> runs once, when the profile is added.
    /// </remarks>
    public class Profile
    {
        private readonly string _profileName;
        private readonly List<Action<IProfileExpression>> _actions = new();
        private readonly Dictionary<(Type, Type, bool), object> _maps = new();
        private bool _configured;
        private bool _allowNullDestinationValues = true;
        private bool _allowNullCollections;
        private INamingConvention _sourceMemberNamingConvention = PascalCaseNamingConvention.Instance;
        private INamingConvention _destinationMemberNamingConvention = PascalCaseNamingConvention.Instance;
        private Func<PropertyInfo, bool> _shouldMapProperty;
        private Func<FieldInfo, bool> _shouldMapField;

        protected Profile()
        {
        }

        protected Profile(string profileName)
        {
            _profileName = profileName;
        }

        /// <summary>The profile's name: by default its type's full name, as in 3.x.</summary>
        public virtual string ProfileName => _profileName ?? GetType().FullName;

        /// <summary>Where a 3.x profile creates its maps. Called once, when the profile is added.</summary>
        protected virtual void Configure()
        {
        }

        internal void EnsureConfigured()
        {
            lock (Mapper.Sync)
            {
                if (_configured)
                    return;
                _configured = true;
            }
            Configure();
        }

        internal void Record(Action<IProfileExpression> action)
        {
            lock (Mapper.Sync)
            {
                _actions.Add(action);
                Mapper.Invalidate();
            }
        }

        internal void Replay(IProfileExpression profile)
        {
            foreach (var action in _actions.ToArray())
                action(profile);
        }

        public LegacyMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
            => CreateMap<TSource, TDestination>(MemberList.Destination);

        /// <summary>
        /// Creates the map, or returns the one already created for these types in this profile, as 3.x did (its
        /// configuration is added to).
        /// </summary>
        public LegacyMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            lock (Mapper.Sync)
            {
                var key = (typeof(TSource), typeof(TDestination), true);
                if (_maps.TryGetValue(key, out var existing))
                    return (LegacyMappingExpression<TSource, TDestination>)existing;
                var map = new LegacyMappingExpression<TSource, TDestination>();
                _maps[key] = map;
                Record(p => map.Replay(p.CreateMap<TSource, TDestination>(memberList)));
                return map;
            }
        }

        public LegacyMappingExpression CreateMap(Type sourceType, Type destinationType)
            => CreateMap(sourceType, destinationType, MemberList.Destination);

        public LegacyMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            lock (Mapper.Sync)
            {
                var key = (sourceType, destinationType, false);
                if (_maps.TryGetValue(key, out var existing))
                    return (LegacyMappingExpression)existing;
                var map = new LegacyMappingExpression(sourceType, destinationType);
                _maps[key] = map;
                Record(p => map.Replay(p.CreateMap(sourceType, destinationType, memberList)));
                return map;
            }
        }

        internal bool HasMap(Type sourceType, Type destinationType)
        {
            lock (Mapper.Sync)
                return _maps.ContainsKey((sourceType, destinationType, true)) || _maps.ContainsKey((sourceType, destinationType, false));
        }

        /// <summary>The source and destination types of the profile's maps, with those <c>ReverseMap</c> adds.</summary>
        internal List<(Type Source, Type Destination)> MapTypes()
        {
            lock (Mapper.Sync)
            {
                var types = new List<(Type, Type)>();
                foreach (var ((source, destination, _), map) in _maps)
                {
                    types.Add((source, destination));
                    if (((IRecordedMap)map).HasReverseMap)
                        types.Add((destination, source));
                }
                return types;
            }
        }

        /// <summary>Whether a null source member gives a null destination member (default true).</summary>
        public bool AllowNullDestinationValues
        {
            get => _allowNullDestinationValues;
            set
            {
                _allowNullDestinationValues = value;
                Record(p => p.AllowNullDestinationValues = value);
            }
        }

        /// <summary>Whether a null source collection gives a null destination collection (default false: empty).</summary>
        public bool AllowNullCollections
        {
            get => _allowNullCollections;
            set
            {
                _allowNullCollections = value;
                Record(p => p.AllowNullCollections = value);
            }
        }

        public INamingConvention SourceMemberNamingConvention
        {
            get => _sourceMemberNamingConvention;
            set
            {
                _sourceMemberNamingConvention = value;
                Record(p => p.SourceMemberNamingConvention = value);
            }
        }

        public INamingConvention DestinationMemberNamingConvention
        {
            get => _destinationMemberNamingConvention;
            set
            {
                _destinationMemberNamingConvention = value;
                Record(p => p.DestinationMemberNamingConvention = value);
            }
        }

        /// <summary>4.x: which properties are mapped.</summary>
        public Func<PropertyInfo, bool> ShouldMapProperty
        {
            get => _shouldMapProperty;
            set
            {
                _shouldMapProperty = value;
                Record(p => p.ShouldMapProperty = value);
            }
        }

        /// <summary>4.x: which fields are mapped.</summary>
        public Func<FieldInfo, bool> ShouldMapField
        {
            get => _shouldMapField;
            set
            {
                _shouldMapField = value;
                Record(p => p.ShouldMapField = value);
            }
        }

        /// <summary>
        /// 4.x: create maps that weren't configured. AutoMapper 12 has no such setting: when it's set on the static
        /// <see cref="Mapper"/>'s configuration, <c>Mapper.Map</c> creates a missing map between the two types it's
        /// called with (not for their members, and not between collections), as <c>DynamicMap</c> does.
        /// </summary>
        public bool CreateMissingTypeMaps { get; set; }

        public void RecognizePrefixes(params string[] prefixes) => Record(p => p.RecognizePrefixes(prefixes));

        public void RecognizePostfixes(params string[] postfixes) => Record(p => p.RecognizePostfixes(postfixes));

        /// <summary>3.x's alias: AutoMapper 12's <c>ReplaceMemberName(original, alias)</c>.</summary>
        public void RecognizeAlias(string original, string alias) => Record(p => p.ReplaceMemberName(original, alias));

        public void ReplaceMemberName(string original, string newValue) => Record(p => p.ReplaceMemberName(original, newValue));

        public void RecognizeDestinationPrefixes(params string[] prefixes) => Record(p => p.RecognizeDestinationPrefixes(prefixes));

        public void RecognizeDestinationPostfixes(params string[] postfixes) => Record(p => p.RecognizeDestinationPostfixes(postfixes));

        public void ClearPrefixes() => Record(p => p.ClearPrefixes());

        public void AddGlobalIgnore(string propertyNameStartingWith) => Record(p => p.AddGlobalIgnore(propertyNameStartingWith));

        public void DisableConstructorMapping() => Record(p => p.DisableConstructorMapping());

        /// <summary>
        /// 3.x: the extension methods of an assembly, as source members. AutoMapper 12 takes them by type, so this
        /// passes each of the assembly's static classes with extension methods.
        /// </summary>
        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            // The application's own assembly: the types that declare extension methods.
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsSealed && type.IsAbstract && !type.IsGenericTypeDefinition && type.IsDefined(typeof(ExtensionAttribute), false))
                    IncludeSourceExtensionMethods(type);
            }
        }

        public void IncludeSourceExtensionMethods(Type type) => Record(p => p.IncludeSourceExtensionMethods(type));

        /// <summary>4.x: runs for each map when the configuration is built.</summary>
        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration) => Record(p => p.Internal().ForAllMaps(configuration));
    }

    /// <summary>A profile created by name (<c>Mapper.CreateProfile</c>).</summary>
    internal sealed class NamedProfile : Profile
    {
        public NamedProfile(string profileName) : base(profileName)
        {
        }
    }

    /// <summary>An AutoMapper 12 profile that replays a 3.x profile.</summary>
    internal sealed class ReplayProfile : AutoMapper.Profile
    {
        public ReplayProfile(Profile profile) : base(profile.ProfileName, profile.Replay)
        {
        }
    }
}
