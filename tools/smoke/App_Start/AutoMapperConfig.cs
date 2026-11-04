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
        public List<StickyNote> Notes { get; set; } = new List<StickyNote>();
    }

    public class Author
    {
        public string Name { get; set; }
    }

    // As Umbraco 7's PropertyTypeBasic, PropertyTypeDisplay and their member versions: the models derive from the
    // types they're mapped from.
    public class Note
    {
        public string Text { get; set; }
    }

    public class NoteModel : Note
    {
        public string Kind { get; set; }
    }

    public class StickyNote : Note
    {
    }

    public class StickyNoteModel : NoteModel
    {
    }

    public class BookModel
    {
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Summary { get; set; }
        public string Shelf { get; set; }
        public IEnumerable<StickyNoteModel> Notes { get; set; }
    }

    public class SummaryResolver : ValueResolver<Book, string>
    {
        protected override string ResolveCore(Book source) => source.Title + " (" + source.Tags.Count + " tags)";
    }

    public static class AutoMapperConfig
    {
        public static void Register()
        {
            // A member given values that already are its elements' type, with a map only between their base types
            // (issue #11, Umbraco 7's MapPropertyGroupBasicToPropertyGroupDisplay): 3.x assigned the elements;
            // AutoMapper 12 maps them through Note -> NoteModel and fails building the configuration.
            Mapper.CreateMap<Note, NoteModel>().ForMember(d => d.Kind, o => o.UseValue("note"));
            Mapper.CreateMap<StickyNote, StickyNoteModel>().ForMember(d => d.Kind, o => o.UseValue("sticky"));
            Mapper.CreateMap<Book, BookModel>()
                .ForMember(d => d.Summary, o => o.ResolveUsing<SummaryResolver>())
                .ForMember(d => d.Shelf, o => o.ResolveUsing(r => r.Context.Options.Items["Shelf"]))
                .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes.Select(Mapper.Map<StickyNoteModel>)));
            Mapper.CreateMap<List<string>, string>().ConvertUsing(tags =>
            {
                var sorted = tags.OrderBy(t => t);
                return string.Join(",", sorted);
            });
        }
    }
}
