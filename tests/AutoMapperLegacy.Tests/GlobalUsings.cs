// What an application written for AutoMapper 3.x adds to compile against webForCore.AutoMapperLegacy: the facade's
// namespace, and aliases for the two names AutoMapper 12 also has (its instance Mapper and its Profile). Nothing else in
// this project refers to AutoMapperLegacy by name, except where the README says an application has to (a class that
// implements 3.x's ITypeConverter<,> or IMappingAction<,> directly).
global using AutoMapperLegacy;
global using Mapper = AutoMapperLegacy.Mapper;
global using Profile = AutoMapperLegacy.Profile;

// The tests share the static Mapper, so they run one at a time.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
