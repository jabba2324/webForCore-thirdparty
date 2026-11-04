// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issues #9, #11). MIT License.
// The lookup it describes is AutoMapper 3.3.1's ConfigurationStore.FindTypeMap, Copyright (c) 2010 Jimmy Bogard, MIT
// License (see NOTICE).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;

namespace AutoMapperLegacy
{
    /// <summary>
    /// The types whose values AutoMapper 3.x assigned where AutoMapper 12 maps them through a map between their base
    /// types (issue #11).
    /// </summary>
    /// <remarks>
    /// <para>
    /// To map a value of type <c>S</c> to <c>D</c> (a member, or an element of a collection), 3.3.1 looked for a map
    /// from <c>S</c>, its interfaces or its base types <i>to <c>D</c></i> (<c>ConfigurationStore.FindTypeMap</c>). With
    /// none, its other mappers ran, and the last of them, <c>AssignableMapper</c>, assigned a value that already was a
    /// <c>D</c>. AutoMapper 12 to 14 also look along <c>D</c>'s base types and interfaces
    /// (<c>MapperConfiguration.GetTypeMap</c>), and take a map to one of them: for a <c>MemberPropertyTypeDisplay</c>
    /// mapped to <c>MemberPropertyTypeDisplay</c>, Umbraco 7's <c>PropertyTypeBasic -&gt; PropertyTypeDisplay</c>.
    /// That map makes a <c>PropertyTypeDisplay</c>, which isn't the member's type: AutoMapper 12 fails building the
    /// configuration (so every map fails), and 13 and 14 throw when mapping it.
    /// </para>
    /// <para>
    /// AutoMapper's lookup can't be changed through its public API, but a map of the type to itself is found before
    /// any other, so the facade adds one that returns the value (<c>ConvertUsing(value =&gt; value)</c>), which is what
    /// 3.x did, for each type that needs it: one that 3.x would assign (no map to it from it, its interfaces or its
    /// base types) and AutoMapper 12 wouldn't (a map from one of those to one of its own base types or interfaces).
    /// Collections still give a new collection of the same elements, as 3.x's <c>EnumerableMapper</c> did. The types
    /// considered are the destinations of the facade's maps, and the types of their members and of those members'
    /// elements, which are the ones AutoMapper maps values to. Maps of AutoMapper 12 profiles aren't looked at. These
    /// maps are the facade's: <c>Mapper.FindTypeMapFor</c> and <c>GetAllTypeMaps</c> leave them out, as 3.x had none.
    /// </para>
    /// </remarks>
    internal static class AssignedTypes
    {
        /// <summary>The types that need a map to themselves, given the facade's maps (their source and destination types).</summary>
        public static IReadOnlyCollection<Type> Find(IReadOnlyCollection<(Type Source, Type Destination)> maps)
        {
            var found = new List<Type>();
            var seen = new HashSet<Type>();
            foreach (var type in Candidates(maps))
            {
                if (!seen.Add(type) || !CanBeAssigned(type))
                    continue;
                var inheritance = Inheritance(type);
                // 3.x found a map for it: AutoMapper 12 finds one too (it looks at the type itself first).
                if (maps.Any(map => inheritance.Any(source => Matches(map, source, type))))
                    continue;
                // AutoMapper 12 takes a map to one of the type's base types or interfaces, where 3.x assigned.
                if (maps.Any(map => inheritance.Any(source => inheritance.Any(destination => destination != type && Matches(map, source, destination)))))
                    found.Add(type);
            }
            return found;
        }

        /// <summary>Adds the map of the type to itself that returns the value.</summary>
        public static void AddMap(IMapperConfigurationExpression configuration, Type type)
        {
            // CreateMap(type, type).ConvertUsing(value => value) would give AutoMapper an object, not the type, for the
            // member or element; the generic map gives it the type. The type is the application's, and the generic
            // class this package's own.
            var map = (IAssignedTypeMap)Activator.CreateInstance(typeof(AssignedTypeMap<>).MakeGenericType(type));
            map.Add(configuration);
        }

        private interface IAssignedTypeMap
        {
            void Add(IMapperConfigurationExpression configuration);
        }

        private sealed class AssignedTypeMap<T> : IAssignedTypeMap
        {
            public void Add(IMapperConfigurationExpression configuration) => configuration.CreateMap<T, T>().ConvertUsing(value => value);
        }

        /// <summary>The types AutoMapper maps values to: the maps' destinations, their members' types and those members' elements.</summary>
        private static IEnumerable<Type> Candidates(IEnumerable<(Type Source, Type Destination)> maps)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            foreach (var (_, destination) in maps)
            {
                if (destination.ContainsGenericParameters)
                    continue;
                yield return destination;
                // The application's own types, as AutoMapper reads them.
                foreach (var property in destination.GetProperties(flags))
                {
                    if (property.GetIndexParameters().Length == 0)
                        yield return Element(property.PropertyType);
                }
                foreach (var field in destination.GetFields(flags))
                    yield return Element(field.FieldType);
            }
        }

        /// <summary>The innermost element type of a collection type (of a collection of collections), or the type.</summary>
        private static Type Element(Type type)
        {
            while (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            {
                var element = type.IsArray
                    ? type.GetElementType()
                    : type.GetInterfaces().Append(type)
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                        ?.GetGenericArguments()[0];
                if (element == null || element == type)
                    break;
                type = element;
            }
            return type;
        }

        /// <summary>
        /// A type 3.x's <c>AssignableMapper</c> was the mapper for: a class or interface other than <c>object</c>, not a
        /// string or a collection (3.x's collection mappers came first). Value types have no base types to go wrong on.
        /// </summary>
        private static bool CanBeAssigned(Type type)
            => !type.IsValueType && type != typeof(object) && type != typeof(string) && !type.ContainsGenericParameters &&
               !typeof(IEnumerable).IsAssignableFrom(type) && !typeof(Delegate).IsAssignableFrom(type);

        /// <summary>The type, its base types and its interfaces: the types AutoMapper and 3.x look up maps along.</summary>
        private static List<Type> Inheritance(Type type)
        {
            var types = new List<Type>();
            for (var current = type; current != null; current = current.BaseType)
                types.Add(current);
            types.AddRange(type.GetInterfaces());
            return types;
        }

        /// <summary>Whether a map is between the two types, directly or as an open generic map (<c>CreateMap(typeof(A&lt;&gt;), typeof(B&lt;&gt;))</c>).</summary>
        private static bool Matches((Type Source, Type Destination) map, Type source, Type destination)
        {
            if (map.Source == source && map.Destination == destination)
                return true;
            return map.Source.IsGenericTypeDefinition && map.Destination.IsGenericTypeDefinition &&
                   source.IsGenericType && destination.IsGenericType &&
                   source.GetGenericTypeDefinition() == map.Source && destination.GetGenericTypeDefinition() == map.Destination;
        }
    }
}
