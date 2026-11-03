// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
// The members' names and documented behaviour follow AutoMapper 3.3.1 and 4.2.1 (QueryableExtensions), Copyright (c)
// 2010 Jimmy Bogard, MIT License (see NOTICE).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x's <c>Project().To&lt;T&gt;()</c> and 4.x's <c>ProjectTo&lt;T&gt;()</c> without a configuration,
    /// with the static <see cref="Mapper"/>'s configuration, on AutoMapper 12's <c>ProjectTo</c>.
    /// </summary>
    public static class LegacyQueryableExtensions
    {
        public static IProjectionExpression Project<TSource>(this IQueryable<TSource> source)
            => new ProjectionExpression(source, null);

        public static IProjectionExpression Project<TSource>(this IQueryable<TSource> source, IMappingEngine mappingEngine)
            => new ProjectionExpression(source, mappingEngine);

        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, object parameters = null,
            params Expression<Func<TDestination, object>>[] membersToExpand)
            => source.ProjectTo(Mapper.ConfigurationProvider, parameters, membersToExpand);

        public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IDictionary<string, object> parameters,
            params string[] membersToExpand)
            => source.ProjectTo<TDestination>(Mapper.ConfigurationProvider, parameters, membersToExpand);
    }

    /// <summary>AutoMapper 3.x's <c>Project()</c>: <c>To&lt;T&gt;()</c> projects with AutoMapper 12.</summary>
    public interface IProjectionExpression
    {
        IQueryable<TResult> To<TResult>(object parameters = null);
        IQueryable<TResult> To<TResult>(IDictionary<string, object> parameters);
        IQueryable<TResult> To<TResult>(object parameters = null, params string[] membersToExpand);
        IQueryable<TResult> To<TResult>(IDictionary<string, object> parameters, params string[] membersToExpand);
        IQueryable<TResult> To<TResult>(object parameters = null, params Expression<Func<TResult, object>>[] membersToExpand);
        IQueryable<TResult> To<TResult>(IDictionary<string, object> parameters, params Expression<Func<TResult, object>>[] membersToExpand);
    }

    internal sealed class ProjectionExpression : IProjectionExpression
    {
        private readonly IQueryable _source;
        private readonly IMappingEngine _engine;

        public ProjectionExpression(IQueryable source, IMappingEngine engine)
        {
            _source = source;
            _engine = engine;
        }

        private IConfigurationProvider Configuration => _engine?.ConfigurationProvider ?? Mapper.ConfigurationProvider;

        public IQueryable<TResult> To<TResult>(object parameters = null)
            => _source.ProjectTo<TResult>(Configuration, parameters);

        public IQueryable<TResult> To<TResult>(IDictionary<string, object> parameters)
            => _source.ProjectTo<TResult>(Configuration, parameters);

        public IQueryable<TResult> To<TResult>(object parameters = null, params string[] membersToExpand)
            => _source.ProjectTo<TResult>(Configuration, ToDictionary(parameters), membersToExpand);

        public IQueryable<TResult> To<TResult>(IDictionary<string, object> parameters, params string[] membersToExpand)
            => _source.ProjectTo<TResult>(Configuration, parameters, membersToExpand);

        public IQueryable<TResult> To<TResult>(object parameters = null, params Expression<Func<TResult, object>>[] membersToExpand)
            => _source.ProjectTo(Configuration, parameters, membersToExpand);

        public IQueryable<TResult> To<TResult>(IDictionary<string, object> parameters, params Expression<Func<TResult, object>>[] membersToExpand)
            => _source.ProjectTo(Configuration, (object)parameters, membersToExpand);

        /// <summary>An anonymous object's properties as a dictionary: AutoMapper 12 takes members by name with one.</summary>
        private static IDictionary<string, object> ToDictionary(object parameters)
        {
            if (parameters == null || parameters is IDictionary<string, object>)
                return (IDictionary<string, object>)parameters;
            // The application's parameters object (usually an anonymous type).
            return parameters.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(parameters));
        }
    }
}
