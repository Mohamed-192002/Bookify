using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using System.Security.Claims;
using Bookify.Web.Services;

namespace subscriperify.Web.Controllers
{
    public class SubscripersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IImageServices imageServices;

        private readonly IMapper mapper;
        public SubscripersController(ApplicationDbContext _context, IMapper _mapper, IImageServices _imageServices)
        {
            context = _context;
            mapper = _mapper;
            imageServices = _imageServices;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View("Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriperFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = PopulateViewModel(model);
                return View("Form", viewModel);
            }
            var subscriper = mapper.Map<Subscriper>(model);

            if (model.Image != null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var result = await imageServices.UploadAsync(model.Image, imageName, "/Images/Subscripers", true);

                if (result.IsUploaded)
                {
                    subscriper.ImageUrl = $"/Images/Subscripers/{imageName}";
                    subscriper.ImageThumbnailUrl = $"/Images/Subscripers/thumb/{imageName}";
                }
                else
                {
                    ModelState.AddModelError(nameof(Image), result.errorMessage!);
                    var viewModel = PopulateViewModel(model);
                    return View("Form", viewModel);
                }
            }

            subscriper.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            context.Subscripers.Add(subscriper);
            context.SaveChanges();

            return RedirectToAction(nameof(Index), new { id = subscriper.Id });

        }
        public IActionResult Details(int id)
        {
            var subscriper = context.Subscripers
                .Include(b => b.Governorate)
                .Include(b => b.Area)
                .SingleOrDefault(b => b.Id == id);
            if (subscriper is null)
                return NotFound();

            var viewModel = mapper.Map<SubscriperViewModel>(subscriper);

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var subscriper = context.Subscripers.Find(id);

            if (subscriper is null)
                return NotFound();

            var model = mapper.Map<SubscriperFormViewModel>(subscriper);
            var viewmodel = PopulateViewModel(model);
            return View("Form", viewmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriperFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = PopulateViewModel(model);
                return View("Form", viewModel);
            }

            var subscriper = context.Subscripers.Find(model.Id);
            if (subscriper is null)
                return NotFound();

            if (model.Image != null)
            {
                if (subscriper.ImageUrl != null)
                    imageServices.Delete(subscriper.ImageUrl, subscriper.ImageThumbnailUrl);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var result = await imageServices.UploadAsync(model.Image, imageName, "/Images/Subscripers", true);

                if (result.IsUploaded)
                {
                    model.ImageUrl = $"/Images/Subscripers/{imageName}";
                    model.ImageThumbnailUrl = $"/Images/Subscripers/thumb/{imageName}";
                }
                else
                {
                    ModelState.AddModelError(nameof(Image), result.errorMessage!);
                    var viewModel = PopulateViewModel(model);
                    return View("Form", viewModel);
                }
            }

            else if (model.ImageUrl is null)
            {
                model.ImageUrl = subscriper.ImageUrl;
                model.ImageThumbnailUrl = subscriper.ImageThumbnailUrl;
            }

            subscriper = mapper.Map(model, subscriper);
            subscriper.LastUpdatedOn = DateTime.Now;
            subscriper.LastUpdatedId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            context.SaveChanges();

            return RedirectToAction(nameof(Index), new { id = subscriper.Id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(SearchFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriper = await context.Subscripers.SingleOrDefaultAsync(
                s => s.NationalId == viewModel.Value
                || s.MobileNumber == viewModel.Value
                || s.Email == viewModel.Value
                );

            var model = mapper.Map<SubscriperFormViewModel>(subscriper);
            return PartialView("_Result", model);

        }
        private SubscriperFormViewModel PopulateViewModel(SubscriperFormViewModel? model = null)
        {
            SubscriperFormViewModel viewModel = model is null ? new SubscriperFormViewModel() : model;

            var governorates = context.Governorates.Where(g => !g.IsDeleted).OrderBy(g => g.Name).ToList();
            viewModel.Governorates = mapper.Map<IEnumerable<SelectListItem>>(governorates);
            if (model?.GovernorateId > 0)
            {
                var areas = context.Areas
                    .Where(a => a.GovernorateId == model.GovernorateId && !a.IsDeleted)
                    .OrderBy(a => a.Name).ToList();
                viewModel.Areas = mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return viewModel;
        }
        [AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var areas = context.Areas.Where(a => a.GovernorateId == governorateId).Select(g => new SelectListItem
            {
                Text = g.Name,
                Value = g.Id.ToString()
            })
                .OrderBy(x => x.Text)
                .ToList();
            return Ok(areas);
        }
        public IActionResult AllowPhoneNumber(SubscriperFormViewModel viewModel)
        {
            var subscriperPhone = context.Subscripers.FirstOrDefault(c => c.MobileNumber == viewModel.MobileNumber);
            var isAllow = subscriperPhone is null || subscriperPhone.Id.Equals(viewModel.Id);
            return Json(isAllow);
        }
        public IActionResult AllowNationalId(SubscriperFormViewModel viewModel)
        {
            var subscriperId = context.Subscripers.FirstOrDefault(c => c.NationalId == viewModel.NationalId);
            var isAllow = subscriperId is null || subscriperId.Id.Equals(viewModel.Id);
            return Json(isAllow);
        }
        public IActionResult AllowEmail(SubscriperFormViewModel viewModel)
        {
            var subscriperEmail = context.Subscripers.FirstOrDefault(c => c.Email == viewModel.Email);
            var isAllow = subscriperEmail is null || subscriperEmail.Id.Equals(viewModel.Id);
            return Json(isAllow);
        }
    }
}
