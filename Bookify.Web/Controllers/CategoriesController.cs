using Microsoft.AspNetCore.Mvc;

namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext context;

        public CategoriesController(ApplicationDbContext _context)
        {
            context = _context;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> categories = context.Categories;
            
            return View(categories);
        }
    }
}
