// Written as code for AutoMapper 3.x was: `using AutoMapper;` and the static Mapper (GlobalUsings.cs has the aliases).
// The resolvers and converters are shaped like Umbraco 7's and SocialGoal's.
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Xunit;

namespace LegacyApp
{
    public class Content
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CreatorId { get; set; }
        public Customer Owner { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class ContentDisplay
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public string OwnerEmail { get; set; }
        public string TreeNodeUrl { get; set; }
        public int TagCount { get; set; }
        public string Culture { get; set; }
        public string Path { get; set; }
    }

    public class CreatorResolver : ValueResolver<Content, string>
    {
        public static Func<int, string> Lookup = id => "user" + id;

        protected override string ResolveCore(Content source) => Lookup(source.CreatorId);
    }

    public class EmailResolver : ValueResolver<Customer, string>
    {
        protected override string ResolveCore(Customer source) => source?.Email;
    }

    public class CountResolver : ValueResolver<IEnumerable<string>, int>
    {
        private readonly int _extra;

        public CountResolver() : this(0)
        {
        }

        public CountResolver(int extra) => _extra = extra;

        protected override int ResolveCore(IEnumerable<string> source) => source.Count() + _extra;
    }

    // An IValueResolver over ResolutionResult, reading the operation's items through the context, as Umbraco's
    // ContentTreeNodeUrlResolver does.
    internal class TreeNodeUrlResolver : IValueResolver
    {
        public ResolutionResult Resolve(ResolutionResult source)
        {
            var culture = source.Context.Options.Items.TryGetValue("Culture", out var value) ? (string)value : "none";
            return source.New("/tree/" + ((Content)source.Value).Id + "?c=" + culture, typeof(string));
        }
    }

    internal class IgnoringResolver : IValueResolver
    {
        public ResolutionResult Resolve(ResolutionResult source) => source.Ignore();
    }

    public class NameConverter : TypeConverter<Content, string>
    {
        protected override string ConvertCore(Content source) => source.Name + "#" + source.Id;
    }

    // A 3.x ITypeConverter implemented directly, as SocialGoal's PagedListConverter is. AutoMapper 12 has an interface of
    // the same name, so this is the one place where the type has to be named with the facade's namespace.
    public class ContentListConverter : AutoMapperLegacy.ITypeConverter<List<Content>, List<ContentDisplay>>
    {
        public List<ContentDisplay> Convert(ResolutionContext context)
        {
            var source = (List<Content>)context.SourceValue;
            return source.Select(Mapper.Map<Content, ContentDisplay>).Reverse().ToList();
        }
    }

    public static class ContentMappingExtensions
    {
        // An application's helper over AutoMapper 12's IMappingExpression<,> (Umbraco has several).
        public static IMappingExpression<TSource, TDest> IgnoreCommon<TSource, TDest>(this IMappingExpression<TSource, TDest> mapping)
            where TDest : ContentDisplay
            => mapping
                .ForMember(d => d.TreeNodeUrl, o => o.Ignore())
                .ForMember(d => d.Path, o => o.ResolveUsing(new PathResolver()));

        private class PathResolver : IValueResolver
        {
            public ResolutionResult Resolve(ResolutionResult source) => source.New("-1," + ((Content)source.Value).Id);
        }
    }

    public class ResolverAndConverterTests : StaticMapperTest
    {
        private static Content NewContent() => new Content
        {
            Id = 3,
            Name = "Home",
            CreatorId = 42,
            Owner = new Customer { Name = "Ada", Email = "ada@example.org" },
            Tags = { "a", "b" },
        };

        [Fact]
        public void ResolveUsing_a_resolver_type_and_instance_FromMember_and_ConstructedBy()
        {
            Mapper.CreateMap<Content, ContentDisplay>()
                .ForMember(d => d.Creator, o => o.ResolveUsing<CreatorResolver>())
                .ForMember(d => d.OwnerEmail, o => o.ResolveUsing<EmailResolver>().FromMember(s => s.Owner))
                .ForMember(d => d.TagCount, o => o.ResolveUsing<CountResolver>().FromMember(s => s.Tags).ConstructedBy(() => new CountResolver(10)))
                .ForMember(d => d.TreeNodeUrl, o => o.ResolveUsing(new TreeNodeUrlResolver()))
                .ForMember(d => d.Culture, o => o.ResolveUsing(s => s.Name.Length))
                .ForMember(d => d.Path, o => o.ResolveUsing(r => "-1," + ((Content)r.Value).Id));

            var display = Mapper.Map<ContentDisplay>(NewContent());

            Assert.Equal("user42", display.Creator);
            Assert.Equal("ada@example.org", display.OwnerEmail);
            Assert.Equal(12, display.TagCount);
            Assert.Equal("/tree/3?c=none", display.TreeNodeUrl);
            Assert.Equal("4", display.Culture);
            Assert.Equal("-1,3", display.Path);
            Mapper.AssertConfigurationIsValid();
        }

        [Fact]
        public void Resolvers_see_the_items_of_the_Map_call()
        {
            Mapper.CreateMap<Content, ContentDisplay>()
                .ForMember(d => d.TreeNodeUrl, o => o.ResolveUsing(new TreeNodeUrlResolver()));

            var display = Mapper.Map<Content, ContentDisplay>(NewContent(), opts => opts.Items["Culture"] = "en-GB");

            Assert.Equal("/tree/3?c=en-GB", display.TreeNodeUrl);
        }

        [Fact]
        public void An_ignoring_resolver_leaves_the_destination_member()
        {
            Mapper.CreateMap<Content, ContentDisplay>().ForMember(d => d.Name, o => o.ResolveUsing(new IgnoringResolver()));

            var display = Mapper.Map(NewContent(), new ContentDisplay { Name = "kept" });

            Assert.Equal("kept", display.Name);
        }

        [Fact]
        public void ResolveUsing_a_resolver_type_with_ConstructServicesUsing()
        {
            var constructed = new List<Type>();
            Mapper.Initialize(cfg =>
            {
                cfg.ConstructServicesUsing(t =>
                {
                    constructed.Add(t);
                    return Activator.CreateInstance(t);
                });
                cfg.CreateMap<Content, ContentDisplay>().ForMember(d => d.Creator, o => o.ResolveUsing(typeof(CreatorResolver)));
            });

            Assert.Equal("user42", Mapper.Map<ContentDisplay>(NewContent()).Creator);
            Assert.Contains(typeof(CreatorResolver), constructed);
        }

        [Fact]
        public void Resolver_errors_name_the_types()
        {
            Mapper.CreateMap<Content, ContentDisplay>().ForMember(d => d.OwnerEmail, o => o.ResolveUsing<EmailResolver>());

            var e = Assert.ThrowsAny<AutoMapperMappingException>(() => Mapper.Map<ContentDisplay>(NewContent()));
            Assert.Contains("expected", e.ToString());
        }

        [Fact]
        public void TypeConverter_and_a_3x_ITypeConverter()
        {
            Mapper.CreateMap<Content, ContentDisplay>();
            Mapper.CreateMap<Content, string>().ConvertUsing<NameConverter>();
            Mapper.CreateMap<List<Content>, List<ContentDisplay>>().ConvertUsing<ContentListConverter>();

            Assert.Equal("Home#3", Mapper.Map<string>(NewContent()));
            var list = Mapper.Map<List<ContentDisplay>>(new List<Content> { NewContent(), new Content { Id = 4 } });
            Assert.Equal(new[] { 4, 3 }, list.Select(c => c.Id));
        }

        [Fact]
        public void ConvertUsing_a_converter_instance()
        {
            Mapper.CreateMap<Content, string>().ConvertUsing(new NameConverter());

            Assert.Equal("Home#3", Mapper.Map<Content, string>(NewContent()));
        }

        [Fact]
        public void Conditions_over_the_context_and_ConstructUsing_over_the_context()
        {
            Mapper.CreateMap<Content, ContentDisplay>()
                .ConstructUsing((ResolutionContext c) => new ContentDisplay { Culture = "made for " + ((Content)c.SourceValue).Id })
                .ForMember(d => d.Name, o => o.Condition((ResolutionContext c) => !c.IsSourceValueNull))
                .ForMember(d => d.Id, o => o.Condition(s => s.Id > 0));

            var display = Mapper.Map(new Content { Id = 0, Name = null }, new ContentDisplay { Id = 9, Name = "kept" });
            Assert.Equal(9, display.Id);
            Assert.Equal("kept", display.Name);
            Assert.Equal("made for 5", Mapper.Map<ContentDisplay>(new Content { Id = 5 }).Culture);
        }

        [Fact]
        public void UseValue_and_extension_methods_over_AutoMapper_12s_interface()
        {
            Mapper.CreateMap<Content, ContentDisplay>()
                .IgnoreCommon()
                .ForMember(d => d.Culture, o => o.UseValue("en"));

            var display = Mapper.Map<ContentDisplay>(NewContent());

            Assert.Equal("en", display.Culture);
            Assert.Equal("-1,3", display.Path);
            Assert.Null(display.TreeNodeUrl);
        }

        [Fact]
        public void SourceValue_outside_3x_code_throws()
        {
            Mapper.CreateMap<Content, ContentDisplay>().ForMember(d => d.Name, o => o.MapFrom<string>((s, d, m, ctx) => (string)ctx.SourceValue));

            Assert.ThrowsAny<Exception>(() => Mapper.Map<ContentDisplay>(NewContent()));
        }
    }
}
