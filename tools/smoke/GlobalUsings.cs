// webForCore.AutoMapperLegacy: AutoMapper 12 has its own (instance) Mapper, so code written for AutoMapper 3.x's static
// Mapper gets the facade's by this alias.
global using AutoMapperLegacy;
global using Mapper = AutoMapperLegacy.Mapper;
