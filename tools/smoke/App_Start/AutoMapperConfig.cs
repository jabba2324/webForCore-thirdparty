// AutoMapper 3.x's static API, as applications written for it configure their maps (SocialGoal, Umbraco 7): AutoMapper
// before 12 doesn't run on .NET, so webForCore.AutoMapperLegacy provides it on AutoMapper 12 (issue #9). Called from
// Application_Start; the Mapped action maps with it.
using System.Collections.Generic;
using System.Linq;
using AutoMapper;

namespace SmokeMvc
{
    public class Book
    {
        public string Title { get; set; }
        public Author Author { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class Author
    {
        public string Name { get; set; }
    }

    public class BookModel
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Summary { get; set; }
        public string Shelf { get; set; }
    }

    public class SummaryResolver : ValueResolver<Book, string>
    {
        protected override string ResolveCore(Book source) => source.Title + " (" + source.Tags.Count + " tags)";
    }

    public static class AutoMapperConfig
    {
        public static void Register()
        {
            Mapper.CreateMap<Book, BookModel>()
                .ForMember(d => d.Summary, o => o.ResolveUsing<SummaryResolver>())
                .ForMember(d => d.Shelf, o => o.ResolveUsing(r => r.Context.Options.Items["Shelf"]));
            Mapper.CreateMap<List<string>, string>().ConvertUsing(tags =>
            {
                var sorted = tags.OrderBy(t => t);
                return string.Join(",", sorted);
            });
        }
    }
}
