// Written as code for AutoMapper 3.x was: `using AutoMapper;` and the static Mapper (GlobalUsings.cs has the aliases).
// Issue #11 (Umbraco 7, ContentTypeModelMapperExtensions.MapPropertyGroupBasicToPropertyGroupDisplay): a member given a
// value that already has its type, where the only maps for that type's family are between its base types.
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Xunit;

namespace LegacyApp
{
    // As Umbraco 7's PropertyTypeBasic, PropertyTypeDisplay, MemberPropertyTypeBasic and MemberPropertyTypeDisplay: the
    // display types derive from the basic ones.
    public class FieldBasic
    {
        public string Alias { get; set; }
    }

    public class FieldDisplay : FieldBasic
    {
        public string View { get; set; }
    }

    public class MemberFieldBasic : FieldBasic
    {
    }

    public class MemberFieldDisplay : FieldDisplay
    {
        public bool MemberCanEdit { get; set; }
    }

    public class FieldGroupBasic<T>
    {
        public IEnumerable<T> Properties { get; set; } = new List<T>();
    }

    public class FieldGroupDisplay<T> : FieldGroupBasic<T>
    {
        public T First { get; set; }
    }

    public class Unrelated
    {
        public string Name { get; set; }
    }

    public class UnrelatedDto
    {
        public string Name { get; set; }
    }

    public class ValueOfTheMemberTypeTests : StaticMapperTest
    {
        private static void CreateFieldMaps()
        {
            Mapper.CreateMap<FieldBasic, FieldDisplay>().ForMember(d => d.View, o => o.UseValue("base"));
            Mapper.CreateMap<MemberFieldBasic, MemberFieldDisplay>()
                .ForMember(d => d.View, o => o.UseValue("member"))
                .ForMember(d => d.MemberCanEdit, o => o.UseValue(true));
        }

        private static FieldGroupBasic<MemberFieldBasic> NewGroup() => new FieldGroupBasic<MemberFieldBasic>
        {
            Properties = new List<MemberFieldBasic> { new MemberFieldBasic { Alias = "a" }, new MemberFieldBasic { Alias = "b" } },
        };

        [Fact]
        public void A_collection_of_the_member_type_is_copied_into_a_list_of_the_same_elements()
        {
            CreateFieldMaps();
            var mapped = new List<MemberFieldDisplay>();
            Mapper.CreateMap<FieldGroupBasic<MemberFieldBasic>, FieldGroupDisplay<MemberFieldDisplay>>()
                .ForMember(d => d.Properties, o => o.MapFrom(s => s.Properties.Select(p => Track(mapped, Mapper.Map<MemberFieldDisplay>(p)))))
                .ForMember(d => d.First, o => o.Ignore());
            Mapper.CreateMap<Unrelated, UnrelatedDto>();

            // AutoMapper 12 failed to build the configuration, so every map failed, this one too.
            Assert.Equal("x", Mapper.Map<UnrelatedDto>(new Unrelated { Name = "x" }).Name);

            var group = Mapper.Map<FieldGroupDisplay<MemberFieldDisplay>>(NewGroup());

            // 3.3.1: no map from MemberFieldDisplay (or its bases) to MemberFieldDisplay, so each element was assigned
            // (AssignableMapper) into a new List<T> (EnumerableMapper), enumerating the source once.
            var properties = Assert.IsType<List<MemberFieldDisplay>>(group.Properties);
            Assert.Equal(new[] { "a", "b" }, properties.Select(p => p.Alias));
            Assert.Equal(mapped, properties);
            Assert.All(properties, p => Assert.Equal("member", p.View));
            Assert.All(properties, p => Assert.True(p.MemberCanEdit));
        }

        [Fact]
        public void The_Umbraco_7_mapping_works_with_MapFrom_and_a_method_group()
        {
            CreateFieldMaps();
            Mapper.CreateMap<FieldGroupBasic<MemberFieldBasic>, FieldGroupDisplay<MemberFieldDisplay>>()
                .ForMember(d => d.Properties, o => o.MapFrom(s => s.Properties.Select(Mapper.Map<MemberFieldDisplay>)))
                .ForMember(d => d.First, o => o.Ignore());

            var group = Mapper.Map<FieldGroupDisplay<MemberFieldDisplay>>(NewGroup());

            Assert.Equal(new[] { "a", "b" }, group.Properties.Select(p => p.Alias));
            Assert.All(group.Properties, p => Assert.Equal("member", p.View));
        }

        [Fact]
        public void ResolveUsing_with_a_value_of_the_member_type()
        {
            CreateFieldMaps();
            Mapper.CreateMap<FieldGroupBasic<MemberFieldBasic>, FieldGroupDisplay<MemberFieldDisplay>>()
                .ForMember(d => d.Properties, o => o.ResolveUsing(s => s.Properties.Select(Mapper.Map<MemberFieldDisplay>).ToList()))
                .ForMember(d => d.First, o => o.Ignore());

            var group = Mapper.Map<FieldGroupDisplay<MemberFieldDisplay>>(NewGroup());

            Assert.Equal(new[] { "a", "b" }, group.Properties.Select(p => p.Alias));
            Assert.All(group.Properties, p => Assert.Equal("member", p.View));
        }

        [Fact]
        public void A_single_value_of_the_member_type_is_assigned()
        {
            CreateFieldMaps();
            var first = new MemberFieldDisplay { Alias = "first", View = "own" };
            Mapper.CreateMap<FieldGroupBasic<MemberFieldBasic>, FieldGroupDisplay<MemberFieldDisplay>>()
                .ForMember(d => d.Properties, o => o.Ignore())
                .ForMember(d => d.First, o => o.MapFrom(s => first));

            var group = Mapper.Map<FieldGroupDisplay<MemberFieldDisplay>>(NewGroup());

            Assert.Same(first, group.First);
            Assert.Equal("own", group.First.View);
        }

        [Fact]
        public void A_map_to_the_type_itself_is_still_used()
        {
            CreateFieldMaps();
            Mapper.CreateMap<MemberFieldDisplay, MemberFieldDisplay>().ForMember(d => d.View, o => o.UseValue("copied"));
            var first = new MemberFieldDisplay { Alias = "first", View = "own" };
            Mapper.CreateMap<FieldGroupBasic<MemberFieldBasic>, FieldGroupDisplay<MemberFieldDisplay>>()
                .ForMember(d => d.Properties, o => o.Ignore())
                .ForMember(d => d.First, o => o.MapFrom(s => first));

            var group = Mapper.Map<FieldGroupDisplay<MemberFieldDisplay>>(NewGroup());

            Assert.NotSame(first, group.First);
            Assert.Equal("first", group.First.Alias);
            Assert.Equal("copied", group.First.View);
        }

        [Fact]
        public void The_type_still_has_no_map_of_its_own()
        {
            CreateFieldMaps();
            Mapper.CreateMap<FieldGroupBasic<MemberFieldBasic>, FieldGroupDisplay<MemberFieldDisplay>>()
                .ForMember(d => d.Properties, o => o.MapFrom(s => s.Properties.Select(Mapper.Map<MemberFieldDisplay>)));

            Assert.Null(Mapper.FindTypeMapFor<MemberFieldDisplay, MemberFieldDisplay>());
            Assert.DoesNotContain(Mapper.GetAllTypeMaps(), m => m.SourceType == typeof(MemberFieldDisplay) && m.DestinationType == typeof(MemberFieldDisplay));
            Assert.Equal(3, Mapper.GetAllTypeMaps().Length);
        }

        private static T Track<T>(List<T> mapped, T value)
        {
            mapped.Add(value);
            return value;
        }
    }
}
