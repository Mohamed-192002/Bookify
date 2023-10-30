﻿using Bookify.Web.Core.Models;
using Bookify.Web.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Bookify.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly Cloudinary cloudinary;
        private List<string> allowedExtensionsImage = new() { ".jpg", ".png", ".jpeg" };
        private int maxSizeForImage = 2097152;
        public BooksController(ApplicationDbContext _context, IMapper _mapper, IWebHostEnvironment _webHostEnvironment, IOptions<CloudinarySettings> _cloudinary)
        {
            context = _context;
            mapper = _mapper;
            webHostEnvironment = _webHostEnvironment;
            Account account = new()
            {
                ApiKey = _cloudinary.Value.ApiKey,
                ApiSecret = _cloudinary.Value.ApiSecret,
                Cloud = _cloudinary.Value.Cloud,
            };
            cloudinary = new Cloudinary(account);
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = PopulateViewModel();
            return View("Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = PopulateViewModel(model);
                return View("Form", viewModel);
            }

            var book = mapper.Map<Book>(model);

            if (model.Image != null)
            {
                if (!allowedExtensionsImage.Contains(Path.GetExtension(model.Image.FileName)))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.AllowImagesExtensions);
                    var viewModel = PopulateViewModel(model);
                    return View("Form", viewModel);
                }
                if (model.Image.Length > maxSizeForImage)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxLength);
                    var viewModel = PopulateViewModel(model);
                    return View("Form", viewModel);
                }


                var path = Path.Combine($"{webHostEnvironment.WebRootPath}/Images/Books", model.Image.FileName);
                var Thumbpath = Path.Combine($"{webHostEnvironment.WebRootPath}/Images/Books/thumb", model.Image.FileName);

                using var fileStream = new FileStream(path, FileMode.Create);
                await model.Image.CopyToAsync(fileStream);
                fileStream.Dispose();
                book.ImageUrl = $"/Images/Books/{model.Image.FileName}";
                book.ImageThumbnailUrl = $"/Images/Books/thumb/{model.Image.FileName}";

                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var hight = image.Height / ratio;
                image.Mutate(i => i.Resize(width: 200, height: (int)hight));
                image.Save(Thumbpath);



                //using var stream=model.Image.OpenReadStream();
                //var uploadParams = new ImageUploadParams()
                //{
                //    File = new FileDescription(model.Image.FileName,stream),
                //    UseFilename = true
                //};
                //var uploadResult = await cloudinary.UploadAsync(uploadParams);
                //book.ImageUrl = uploadResult.SecureUrl.ToString();
                //book.ImagePuplicId = uploadResult.PublicId;
            }
            foreach (var item in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = item });



            context.Books.Add(book);
            context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = context.Books.Include(c => c.Categories).SingleOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();

            var model = mapper.Map<BookFormViewModel>(book);
            model.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList();
            var viewmodel = PopulateViewModel(model);
            return View("Form", viewmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel = PopulateViewModel(model);
                return View("Form", viewModel);
            }

            var book = context.Books.Include(c => c.Categories).SingleOrDefault(b => b.Id == model.Id);
            if (book is null)
                return NotFound();

            //  string ImagePuplicId = null;

            if (model.Image != null)
            {
                if (book.ImageUrl != null)
                {
                    var oldPath = $"{webHostEnvironment.WebRootPath}{book.ImageUrl}";
                    var oldThumbPath = $"{webHostEnvironment.WebRootPath}{book.ImageThumbnailUrl}";

                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    if (System.IO.File.Exists(oldThumbPath))
                        System.IO.File.Delete(oldThumbPath);

                    //await cloudinary.DeleteResourcesAsync(book.ImagePuplicId);
                }
                if (!allowedExtensionsImage.Contains(Path.GetExtension(model.Image.FileName)))
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.AllowImagesExtensions);
                    var viewModel = PopulateViewModel(model);
                    return View("Form", viewModel);
                }
                if (model.Image.Length > maxSizeForImage)
                {
                    ModelState.AddModelError(nameof(model.Image), Errors.MaxLength);
                    var viewModel = PopulateViewModel(model);
                    return View("Form", viewModel);
                }

                var path = Path.Combine($"{webHostEnvironment.WebRootPath}/Images/Books", model.Image.FileName);
                var Thumbpath = Path.Combine($"{webHostEnvironment.WebRootPath}/Images/Books/thumb", model.Image.FileName);

                using var fileStream = new FileStream(path, FileMode.Create);
                await model.Image.CopyToAsync(fileStream);
                fileStream.Dispose();

                model.ImageUrl = $"/Images/Books/{model.Image.FileName}";
                model.ImageThumbnailUrl = $"/Images/Books/thumb/{model.Image.FileName}";

                using var image = Image.Load(model.Image.OpenReadStream());
                var ratio = (float)image.Width / 200;
                var hight = image.Height / ratio;
                image.Mutate(i => i.Resize(width: 200, height: (int)hight));
                image.Save(Thumbpath);


                //using var stream = model.Image.OpenReadStream();
                //var uploadParams = new ImageUploadParams()
                //{
                //    File = new FileDescription(model.Image.FileName, stream),
                //    UseFilename = true
                //};
                //var uploadResult = await cloudinary.UploadAsync(uploadParams);
                //model.ImageUrl = uploadResult.SecureUrl.ToString();
                //ImagePuplicId = uploadResult.PublicId;

            }
            else if (model.ImageUrl != null)
                model.ImageUrl = book.ImageUrl;

            book = mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;
            //  book.ImagePuplicId = ImagePuplicId;

            foreach (var item in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = item });

            context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult AllowBook(BookFormViewModel viewModel)
        {
            var isAllow = true;
            var isExist = context.Books.Any(b => b.Title == viewModel.Title && b.AuthorId == viewModel.AuthorId);
            if (viewModel.Id == 0)
                isAllow = isExist;
            else if (viewModel.Id > 0)
            {
                var book = context.Books.Find(viewModel.Id);
                if (book.Title == viewModel.Title)
                    isAllow = false;
                else
                    isAllow = isExist;
            }
            return Json(!isAllow);
        }
        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel;

            if (model is null)
                viewModel = new BookFormViewModel();
            else
                viewModel = model;

            var authors = context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToList();

            viewModel.Authors = mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = mapper.Map<IEnumerable<SelectListItem>>(categories);

            return viewModel;
        }
    }
}
