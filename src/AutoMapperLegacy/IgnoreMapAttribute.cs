// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The attribute's name and documented behaviour follow AutoMapper 3.3.1 (IgnoreMapAttribute.cs), Copyright (c) 2010
// Jimmy Bogard, MIT License (see NOTICE).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x's attribute: the member isn't mapped, and configuration validation doesn't report it. On a
    /// destination member it applies to every map to its class; on a source member, to the maps from its class that
    /// would map it by name (a <c>ReverseMap</c>). AutoMapper 12's own <c>[Ignore]</c> only applies to <c>[AutoMap]</c>
    /// maps, so the facade ignores these members itself.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreMapAttribute : Attribute
    {
        /// <summary>The destination members a map ignores because of the attribute (the application's own types).</summary>
        internal static IEnumerable<string> IgnoredMembers(Type sourceType, Type destinationType)
        {
            if (destinationType == null || destinationType.ContainsGenericParameters)
                return Array.Empty<string>();
            var ignoredInSource = sourceType == null || sourceType.ContainsGenericParameters
                ? new HashSet<string>()
                : new HashSet<string>(Members(sourceType).Where(IsIgnored).Select(m => m.Name));
            return Members(destinationType)
                .Where(m => IsWritable(m) && (IsIgnored(m) || ignoredInSource.Contains(m.Name)))
                .Select(m => m.Name)
                .Distinct()
                .ToArray();
        }

        private static IEnumerable<MemberInfo> Members(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            return type.GetProperties(flags).Cast<MemberInfo>().Concat(type.GetFields(flags));
        }

        private static bool IsIgnored(MemberInfo member) => member.IsDefined(typeof(IgnoreMapAttribute), inherit: true);

        private static bool IsWritable(MemberInfo member)
            => member is FieldInfo || (member is PropertyInfo property && property.GetIndexParameters().Length == 0);
    }
}
