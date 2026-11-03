// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
using System;
using System.Linq.Expressions;
using AutoMapper;

namespace AutoMapperLegacy
{
    /// <summary>
    /// AutoMapper 3.x's map members that AutoMapper 12 removed, as extension methods on AutoMapper 12's
    /// <see cref="IMappingExpression{TSource,TDestination}"/>: for code that has 12's interface rather than the
    /// facade's <see cref="LegacyMappingExpression{TSource,TDestination}"/> (an application's own extension methods
    /// over it). On 12's interface, a lambda passed to <c>ConvertUsing</c> or <c>ConstructUsing</c> becomes an
    /// expression tree, so it can't have a statement body.
    /// </summary>
    public static class MappingExpressionExtensions
    {
        public static void ConvertUsing<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression,
            ITypeConverter<TSource, TDestination> converter)
            => expression.ConvertUsing(Converters.Adapt(converter));

        public static IMappingExpression<TSource, TDestination> ConstructUsing<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> expression, Func<ResolutionContext, TDestination> ctor)
            => expression.ConstructUsing((source, context) =>
                LegacyFrame.Run(context, source, typeof(TSource), null, typeof(TDestination), null, () => ctor(context)));

        public static void ProjectUsing<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression,
            Expression<Func<TSource, TDestination>> projectionExpression)
            => expression.ConvertUsing(projectionExpression);
    }

    /// <summary>
    /// AutoMapper 4.x members of the map that would conflict with 3.x's as overloads, as extension methods: C# only
    /// looks at them when no member of the map applies, so 3.x's code binds as it did in 3.x.
    /// </summary>
    public static class LegacyMappingExpressionExtensions
    {
        /// <summary>
        /// 4.x's conversion over the context (<c>context.SourceValue</c>, <c>context.Options.Items</c>). It's used when
        /// the lambda doesn't compile as 3.x's <c>ConvertUsing(Func&lt;TSource, TDestination&gt;)</c>.
        /// </summary>
        public static void ConvertUsing<TSource, TDestination>(this LegacyMappingExpression<TSource, TDestination> expression,
            Func<ResolutionContext, TDestination> mappingFunction)
            => expression.ConvertUsingContext(mappingFunction);

        /// <summary>
        /// 3.x's construction over the context (<c>context.SourceValue</c>). It's used when the lambda doesn't compile as
        /// <c>ConstructUsing(Func&lt;TSource, TDestination&gt;)</c>.
        /// </summary>
        public static LegacyMappingExpression<TSource, TDestination> ConstructUsing<TSource, TDestination>(
            this LegacyMappingExpression<TSource, TDestination> expression, Func<ResolutionContext, TDestination> ctor)
            => expression.ConstructUsingContext(ctor);
    }
}
