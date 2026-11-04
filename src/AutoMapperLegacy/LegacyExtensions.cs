// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 (TypeMap, ISourceMemberConfigurationExpression,
// QueryableExtensions.Extensions.CreateMapExpression), Copyright (c) 2010 Jimmy Bogard, MIT License (see NOTICE).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Configuration;
using AutoMapper.QueryableExtensions;

namespace AutoMapperLegacy
{
    /// <summary>AutoMapper 3.x members that AutoMapper 12 renamed, as extension methods.</summary>
    public static class LegacyExtensions
    {
        /// <summary>3.x's <c>TypeMap.GetPropertyMaps()</c>: AutoMapper 12's <see cref="TypeMap.PropertyMaps"/>.</summary>
        public static PropertyMap[] GetPropertyMaps(this TypeMap typeMap) => typeMap.PropertyMaps.ToArray();

        /// <summary>3.x's <c>ForSourceMember(..., o =&gt; o.Ignore())</c>: AutoMapper 12's <c>DoNotValidate()</c>.</summary>
        public static void Ignore(this ISourceMemberConfigurationExpression options) => options.DoNotValidate();

        /// <summary>
        /// 3.x's <c>Mapper.Engine.CreateMapExpression&lt;TSource, TDestination&gt;()</c>: the projection AutoMapper 12
        /// builds for <c>ProjectTo</c>, taken from a projection of an empty query.
        /// </summary>
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(this IMappingEngine mappingEngine,
            IDictionary<string, object> parameters = null, params string[] membersToExpand)
        {
            var query = Enumerable.Empty<TSource>().AsQueryable().ProjectTo<TDestination>(mappingEngine.ConfigurationProvider, parameters, membersToExpand);
            // ProjectTo's query is source.Select(projection), with the projection quoted.
            if (query.Expression is MethodCallExpression call && call.Arguments.Count == 2 &&
                call.Arguments[1] is UnaryExpression { NodeType: ExpressionType.Quote } quote &&
                quote.Operand is Expression<Func<TSource, TDestination>> projection)
            {
                return projection;
            }
            throw new NotSupportedException($"AutoMapper's projection from {typeof(TSource)} to {typeof(TDestination)} isn't a single Select.");
        }
    }
}
