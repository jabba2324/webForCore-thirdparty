using System.Linq;
using System.Web.Mvc;

namespace SmokeMvc.Controllers
{
    // A Razor view without dependency injection: compiled with webForCore's Razor although Unity.Mvc brings
    // Microsoft.AspNet.Razor 3.2.7 (webForCore issue #23).
    public class PlainController : Controller
    {
        public ActionResult Index() => View((object)"plain");

        // AutoMapper 3.x's static Mapper (webForCore.AutoMapperLegacy), with the maps Application_Start created.
        public ActionResult Mapped()
        {
            var book = new Book { Title = "Dune", Author = new Author { Name = "Herbert" }, Tags = { "sf", "classic" }, Notes = { new StickyNote { Text = "reread" } } };
            var model = Mapper.Map<Book, BookModel>(book, opts => opts.Items["Shelf"] = "B2");
            var tags = Mapper.Map<string>(book.Tags);
            return Content($"AutoMapperLegacy: {model.Title} by {model.AuthorName}; {model.Summary}; shelf {model.Shelf}; tags {tags}; notes {string.Join(",", model.Notes.Select(n => n.Kind + ":" + n.Text))}", "text/plain");
        }
    }
}
