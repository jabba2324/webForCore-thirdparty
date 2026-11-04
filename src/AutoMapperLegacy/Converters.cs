// webForCore.AutoMapperLegacy: AutoMapper 3.x/4.x's static API on AutoMapper 12-14 (issue #9). MIT License.
using System;
using System.Linq;
using AutoMapper;

namespace AutoMapperLegacy
{
    /// <summary>Adapts 3.x converters and before/after map actions to AutoMapper 12's.</summary>
    internal static class Converters
    {
        public static AutoMapper.ITypeConverter<TSource, TDestination> Adapt<TSource, TDestination>(ITypeConverter<TSource, TDestination> converter)
            => new LegacyTypeConverter<TSource, TDestination>(() => converter);

        /// <summary>A 3.x converter type, constructed for each conversion as 3.x did (ConstructServicesUsing).</summary>
        public static AutoMapper.ITypeConverter<TSource, TDestination> Adapt<TSource, TDestination>(Type converterType)
            => new LegacyTypeConverter<TSource, TDestination>(() => (ITypeConverter<TSource, TDestination>)Mapper.ConstructService(converterType));

        /// <summary>
        /// For <c>CreateMap(Type, Type).ConvertUsing(Type)</c>: a 3.x converter type becomes an AutoMapper 12 converter
        /// type that wraps it; any other type is AutoMapper 12's to handle.
        /// </summary>
        public static Type AdaptType(Type converterType, Type sourceType, Type destinationType)
        {
            if (converterType.ContainsGenericParameters)
                return converterType;
            // The application's converter type: which 3.x ITypeConverter<,> it implements (the one for the map's types,
            // if it implements several), to close the adapter over it.
            var legacies = converterType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITypeConverter<,>))
                .ToArray();
            var legacy = legacies.FirstOrDefault(i => i.GetGenericArguments()[0] == sourceType && i.GetGenericArguments()[1] == destinationType)
                         ?? legacies.FirstOrDefault();
            if (legacy == null)
                return converterType;
            var arguments = legacy.GetGenericArguments();
            return typeof(LegacyTypeConverterOf<,,>).MakeGenericType(arguments[0], arguments[1], converterType);
        }

        public static Action<TSource, TDestination, ResolutionContext> Action<TSource, TDestination>(Type actionType)
        {
            if (typeof(IMappingAction<TSource, TDestination>).IsAssignableFrom(actionType))
                return (source, destination, context) =>
                    ((IMappingAction<TSource, TDestination>)Mapper.ConstructService(actionType)).Process(source, destination);
            if (typeof(AutoMapper.IMappingAction<TSource, TDestination>).IsAssignableFrom(actionType))
                return (source, destination, context) =>
                    ((AutoMapper.IMappingAction<TSource, TDestination>)Mapper.ConstructService(actionType)).Process(source, destination, context);
            throw new ArgumentException($"{actionType} isn't an IMappingAction<{typeof(TSource)}, {typeof(TDestination)}>.", nameof(actionType));
        }
    }

    /// <summary>Calls a 3.x converter with the source and destination in the context, as 3.x passed them.</summary>
    internal class LegacyTypeConverter<TSource, TDestination> : AutoMapper.ITypeConverter<TSource, TDestination>
    {
        private readonly Func<ITypeConverter<TSource, TDestination>> _converter;

        public LegacyTypeConverter(Func<ITypeConverter<TSource, TDestination>> converter) => _converter = converter;

        public TDestination Convert(TSource source, TDestination destination, ResolutionContext context)
            => LegacyFrame.Run(context, source, typeof(TSource), destination, typeof(TDestination), null,
                () => _converter().Convert(context));
    }

    /// <summary>
    /// <see cref="LegacyTypeConverter{TSource,TDestination}"/> for a converter type, as a type AutoMapper 12 constructs.
    /// </summary>
    internal sealed class LegacyTypeConverterOf<TSource, TDestination, TConverter> : LegacyTypeConverter<TSource, TDestination>
        where TConverter : ITypeConverter<TSource, TDestination>
    {
        public LegacyTypeConverterOf()
            : base(() => (TConverter)Mapper.ConstructService(typeof(TConverter)))
        {
        }
    }
}
