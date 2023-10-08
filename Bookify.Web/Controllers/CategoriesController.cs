using Bookify.Web.Core.Models;

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

        public IActionResult Create()
        {
            var viewModel = new CategoryFormViewModel();
            return View("CreateAndEdit", viewModel);
        }
        [HttpPost]
        public IActionResult Create(CategoryFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View("CreateAndEdit", viewModel);
            var model=new Category { Name = viewModel.Name };
            context.Categories.Add(model);
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(int id)
        {
            var category = context.Categories.Find(id);
            if (category == null)
                return NotFound();
            var viewModel = new CategoryFormViewModel()
            {
                Id = id,
                Name = category.Name,
            };
            return View("CreateAndEdit",viewModel);
        }
        [HttpPost]
        public IActionResult Edit(CategoryFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View("CreateAndEdit", viewModel);

            var category = context.Categories.Find(viewModel.Id);
            category.Name = viewModel.Name;
            category.LastUpdatedOn = DateTime.Now;
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
  