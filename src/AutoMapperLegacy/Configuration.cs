// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 and 4.2.1 (IConfiguration, IProfileExpression,
// IMapperConfiguration), Copyright (c) 2010 Jimmy Bogard, MIT License (see NOTICE).
using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x's configuration, which <c>Mapper.Initialize</c> passes to its lambda and
    /// <see cref="Mapper.Configuration"/> returns: the static <see cref="Mapper"/>'s maps, profiles and settings.
    /// </summary>
    public interface IConfiguration
    {
        LegacyMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();
        LegacyMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList);
        LegacyMappingExpression CreateMap(Type sourceType, Type destinationType);
        LegacyMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList);

        Profile CreateProfile(string profileName);
        void CreateProfile(string profileName, Action<Profile> profileConfiguration);
        void AddProfile(Profile profile);
        /// <summary>An AutoMapper 12 profile, as 4.2-style code may already have.</summary>
        void AddProfile(AutoMapper.Profile profile);
        /// <summary>A 3.x profile (<see cref="Profile"/>) or an AutoMapper 12 one.</summary>
        void AddProfile<TProfile>() where TProfile : new();
        void AddProfile(Type profileType);

        void ConstructServicesUsing(Func<Type, object> constructor);
        /// <summary>3.x compiled the maps here; the facade builds them on first use, so this does nothing.</summary>
        void Seal();

        bool AllowNullDestinationValues { get; set; }
        bool AllowNullCollections { get; set; }
        bool CreateMissingTypeMaps { get; set; }
        INamingConvention SourceMemberNamingConvention { get; set; }
        INamingConvention DestinationMemberNamingConvention { get; set; }
        Func<PropertyInfo, bool> ShouldMapProperty { get; set; }
        Func<FieldInfo, bool> ShouldMapField { get; set; }
        void RecognizePrefixes(params string[] prefixes);
        void RecognizePostfixes(params string[] postfixes);
        void RecognizeAlias(string original, string alias);
        void ReplaceMemberName(string original, string newValue);
        void RecognizeDestinationPrefixes(params string[] prefixes);
        void RecognizeDestinationPostfixes(params string[] postfixes);
        void ClearPrefixes();
        void AddGlobalIgnore(string propertyNameStartingWith);
        void DisableConstructorMapping();
        void IncludeSourceExtensionMethods(Assembly assembly);
        void ForAllMaps(Action<TypeMap, IMappingExpression> configuration);
    }

    /// <summary>
    /// The static <see cref="Mapper"/>'s configuration: the default profile (its maps and settings go to AutoMapper
    /// 12's root configuration), the profiles added to it and the global settings.
    /// </summary>
    internal sealed class RootConfiguration : Profile, IConfiguration
    {
        private readonly List<Action<IMapperConfigurationExpression>> _globalActions = new();
        private readonly List<Profile> _profiles = new();
        private readonly List<AutoMapper.Profile> _nativeProfiles = new();
        private readonly Dictionary<string, Profile> _namedProfiles = new();

        public RootConfiguration() : base(string.Empty)
        {
        }

        public Func<Type, object> ServiceCtor { get; private set; }

        /// <summary>
        /// Builds AutoMapper 12's configuration from what's registered. <paramref name="assignedTypes"/> are the types
        /// the facade gave a map to themselves that returns the value, as 3.x assigned it (<see cref="AssignedTypes"/>,
        /// issue #11).
        /// </summary>
        public MapperConfiguration Build(out IReadOnlyCollection<Type> assignedTypes)
        {
            lock (Mapper.Sync)
            {
                var maps = MapTypes();
                foreach (var profile in _profiles)
                    maps.AddRange(profile.MapTypes());
                var assigned = AssignedTypes.Find(maps);
                assignedTypes = assigned;
                return new MapperConfiguration(cfg =>
                {
                    foreach (var action in _globalActions)
                        action(cfg);
                    Replay(cfg);
                    foreach (var profile in _profiles)
                        cfg.AddProfile(new ReplayProfile(profile));
                    foreach (var profile in _nativeProfiles)
                        cfg.AddProfile(profile);
                    foreach (var type in assigned)
                        AssignedTypes.AddMap(cfg, type);
                });
            }
        }

        public Profile CreateProfile(string profileName)
        {
            lock (Mapper.Sync)
            {
                if (_namedProfiles.TryGetValue(profileName, out var existing))
                    return existing;
                var profile = new NamedProfile(profileName);
                _namedProfiles[profileName] = profile;
                AddProfile(profile);
                return profile;
            }
        }

        public void CreateProfile(string profileName, Action<Profile> profileConfiguration)
            => profileConfiguration(CreateProfile(profileName));

        public void AddProfile(Profile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            lock (Mapper.Sync)
            {
                _profiles.Add(profile);
                Mapper.Invalidate();
            }
            // As 3.x did: the profile's Configure runs when it's added (it may call Mapper.CreateMap).
            profile.EnsureConfigured();
        }

        public void AddProfile(AutoMapper.Profile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));
            lock (Mapper.Sync)
            {
                _nativeProfiles.Add(profile);
                Mapper.Invalidate();
            }
        }

        public void AddProfile<TProfile>() where TProfile : new() => AddProfileInstance(new TProfile());

        public void AddProfile(Type profileType) => AddProfileInstance(Activator.CreateInstance(profileType, nonPublic: true));

        private void AddProfileInstance(object profile)
        {
            switch (profile)
            {
                case Profile legacy:
                    AddProfile(legacy);
                    break;
                case AutoMapper.Profile native:
                    AddProfile(native);
                    break;
                default:
                    throw new ArgumentException($"{profile?.GetType()} isn't a profile.", nameof(profile));
            }
        }

        public void ConstructServicesUsing(Func<Type, object> constructor)
        {
            lock (Mapper.Sync)
            {
                ServiceCtor = constructor;
                _globalActions.Add(cfg => cfg.ConstructServicesUsing(constructor));
                Mapper.Invalidate();
            }
        }

        public void Seal()
        {
        }
    }
}
