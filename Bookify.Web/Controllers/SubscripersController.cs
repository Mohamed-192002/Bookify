using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;
using System.Security.Claims;
using Bookify.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WhatsAppCloudApi.Services;
using WhatsAppCloudApi;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace subscriperify.Web.Controllers
{
    public class SubscripersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageServices _imageServices;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;


        public SubscripersController(ApplicationDbContext context, IMapper mapper, IImageServices imageServices, IDataProtectionProvider dataProtector, IWhatsAppClient whatsAppClient, IWebHostEnvironment webHostEnvironment, IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender)
        {
            _context = context;
            _mapper = mapper;
            _imageServices = imageServices;
            _dataProtector = dataProtector.CreateProtector("DataProtectionDemo");
            _whatsAppClient = whatsAppClient;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
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
            var subscriper = _mapper.Map<Subscriper>(model);

            if (model.Image != null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var result = await _imageServices.UploadAsync(model.Image, imageName, "/Images/Subscripers", true);

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

            _context.Subscripers.Add(subscriper);
            _context.SaveChanges();
            // Send welcome email to user
            var Placeholders = new Dictionary<string, string>()
                     {
                         {"imageUrl","https://res.cloudinary.com/mhmdnosair/image/upload/v1700498236/icon-positive-vote-2_sgatwf.png"},
                         {"header",$"Hey {model.FirstName}, thanks for joining us!" },
                         {"body","Welcome, for Joining Bookify" },
                     };
            #region Send massage Email
            var body = _emailBodyBuilder.GetEmailBody("notification", Placeholders);
            await _emailSender.SendEmailAsync(model.Email, "Welcome to Bookify", body);
            #endregion

            // send welcome message using whatsapp
            if (model.HasWhatsApp)
            {
                var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01068103118" : model.MobileNumber;
                await _whatsAppClient
                    .SendMessage(mobile: $"2{mobileNumber}", language: WhatsAppLanguageCode.English_US, template: "welcome_massage ");
            }
            return RedirectToAction(nameof(Index), new { id = _dataProtector.Protect(subscriper.Id.ToString()) });

        }
        public IActionResult Details(string id)
        {
            int subscriperId = int.Parse(_dataProtector.Unprotect(id));
            var subscriper = _context.Subscripers
                .Include(b => b.Governorate)
                .Include(b => b.Area)
                .SingleOrDefault(b => b.Id == subscriperId);
            if (subscriper is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriperViewModel>(subscriper);
            viewModel.Key = id;
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            int subscriperId = int.Parse(_dataProtector.Unprotect(id));

            var subscriper = _context.Subscripers.Find(subscriperId);

            if (subscriper is null)
                return NotFound();

            var model = _mapper.Map<SubscriperFormViewModel>(subscriper);
            var viewmodel = PopulateViewModel(model);
            viewmodel.Key = id;
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

            int subscriperId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriper = _context.Subscripers.Find(subscriperId);
            if (subscriper is null)
                return NotFound();

            if (model.Image != null)
            {
                if (subscriper.ImageUrl != null)
                    _imageServices.Delete(subscriper.ImageUrl, subscriper.ImageThumbnailUrl);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var result = await _imageServices.UploadAsync(model.Image, imageName, "/Images/Subscripers", true);

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

            subscriper = _mapper.Map(model, subscriper);
            subscriper.LastUpdatedOn = DateTime.Now;
            subscriper.LastUpdatedId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new { id = model.Key });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(SearchFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriper = await _context.Subscripers.SingleOrDefaultAsync(
                s => s.NationalId == viewModel.Value
                || s.MobileNumber == viewModel.Value
                || s.Email == viewModel.Value
                );

            var model = _mapper.Map<SubscriperSearchResultViewModel>(subscriper);
            if (subscriper != null)
                model.Key = _dataProtector.Protect(subscriper.Id.ToString());
            return PartialView("_Result", model);

        }
        private SubscriperFormViewModel PopulateViewModel(SubscriperFormViewModel? model = null)
        {
            SubscriperFormViewModel viewModel = model is null ? new SubscriperFormViewModel() : model;

            var governorates = _context.Governorates.Where(g => !g.IsDeleted).OrderBy(g => g.Name).ToList();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);
            if (model?.GovernorateId > 0)
            {
                var areas = _context.Areas
                    .Where(a => a.GovernorateId == model.GovernorateId && !a.IsDeleted)
                    .OrderBy(a => a.Name).ToList();
                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }

            return viewModel;
        }
        [AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var areas = _context.Areas.Where(a => a.GovernorateId == governorateId).Select(g => new SelectListItem
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
            int id = 0;
            if (!string.IsNullOrEmpty(viewModel.Key))
                id = int.Parse(_dataProtector.Unprotect(viewModel.Key));
            var subscriperPhone = _context.Subscripers.FirstOrDefault(c => c.MobileNumber == viewModel.MobileNumber);
            var isAllow = subscriperPhone is null || subscriperPhone.Id.Equals(id);
            return Json(isAllow);
        }
        public IActionResult AllowNationalId(SubscriperFormViewModel viewModel)
        {
            int id = 0;
            if (!string.IsNullOrEmpty(viewModel.Key))
                id = int.Parse(_dataProtector.Unprotect(viewModel.Key));
            var subscriperId = _context.Subscripers.FirstOrDefault(c => c.NationalId == viewModel.NationalId);
            var isAllow = subscriperId is null || subscriperId.Id.Equals(id);
            return Json(isAllow);
        }
        public IActionResult AllowEmail(SubscriperFormViewModel viewModel)
        {
            int id = 0;
            if (!string.IsNullOrEmpty(viewModel.Key))
                id = int.Parse(_dataProtector.Unprotect(viewModel.Key));
            var subscriperEmail = _context.Subscripers.FirstOrDefault(c => c.Email == viewModel.Email);
            var isAllow = subscriperEmail is null || subscriperEmail.Id.Equals(id);
            return Json(isAllow);
        }
    }
}
