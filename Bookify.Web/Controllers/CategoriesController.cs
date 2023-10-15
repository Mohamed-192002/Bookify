namespace Bookify.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CategoriesController(ApplicationDbContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }
        public IActionResult Index()
        {
            var categories = context.Categories.AsNoTracking().ToList();

            var viewModelMapping = mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            return View(viewModelMapping);
        }
        [AjaxOnly]
        public IActionResult Create()
        {
            var viewModel = new CategoryFormViewModel();
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var model = mapper.Map<Category>(viewModel);

            context.Categories.Add(model);
            context.SaveChanges();

            var categoryViewModel = mapper.Map<CategoryViewModel>(model);

            return PartialView("_CategoryRow", categoryViewModel);
        }
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var category = context.Categories.Find(id);
            if (category == null)
                return NotFound();
            var viewModel = mapper.Map<CategoryFormViewModel>(category);
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var category = context.Categories.Find(viewModel.Id);
            category = mapper.Map(viewModel, category);
            category.LastUpdatedOn = DateTime.Now;
            context.SaveChanges();

            var categoryViewModel = mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", categoryViewModel);
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var category = context.Categories.Find(id);
            if (category == null)
                return NotFound();
            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;
            context.SaveChanges();
            return Ok(category.LastUpdatedOn.ToString());
        }
        public IActionResult AllowCategory(CategoryFormViewModel viewModel)
        {
            var isExist = context.Categories.Any(c => c.Name == viewModel.Name);
            return Json(!isExist);
        }
    }
}
