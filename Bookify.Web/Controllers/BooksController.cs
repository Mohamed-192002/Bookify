﻿using Bookify.Web.Core.Models;
using Bookify.Web.Services;
using Bookify.Web.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IImageServices imageServices;
        //private readonly Cloudinary cloudinary;
        private List<string> allowedExtensionsImage = new() { ".jpg", ".png", ".jpeg" };
        private int maxSizeForImage = 2097152;
        public BooksController(ApplicationDbContext _context, IMapper _mapper, IWebHostEnvironment _webHostEnvironment, IImageServices _imageServices/*, IOptions<CloudinarySettings> _cloudinary*/)
        {
            context = _context;
            mapper = _mapper;
            webHostEnvironment = _webHostEnvironment;
            imageServices = _imageServices;
            //Account account = new()
            //{
            //    ApiKey = _cloudinary.Value.ApiKey,
            //    ApiSecret = _cloudinary.Value.ApiSecret,
            //    Cloud = _cloudinary.Value.Cloud,
            //};
            //cloudinary = new Cloudinary(account);
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetBooks()
        {


            var skip = int.Parse(Request.Form["start"]);
            var take = int.Parse(Request.Form["length"]);

            var SearchValue = Request.Form["search[value]"];

            IQueryable<Book> books = context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);


            if (!string.IsNullOrEmpty(SearchValue))
                books = books.Where(b => b.Title.Contains(SearchValue) || b.Author!.Name.Contains(SearchValue));

            var SortColumnIndex = Request.Form["order[0][column]"];
            var SortColumn = Request.Form[$"columns[{SortColumnIndex}][name]"];
            var SortDir = Request.Form["order[0][dir]"];

            books = books.OrderBy($"{SortColumn} {SortDir}");

            var data = books.Skip(skip).Take(take).ToList();

            var mapData = mapper.Map<IEnumerable<BookViewModel>>(data);
            var recordTotal = books.Count();

            return Ok(new
            {
                recordsFiltered = recordTotal,
                recordTotal,
                data = mapData
            });
        }
        public IActionResult Details(int id)
        {
            var book = context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .Include(b => b.BookCopies)
                .SingleOrDefault(b => b.Id == id);
            if (book is null)
                return NotFound();

            var viewModel = mapper.Map<BookViewModel>(book);

            return View(viewModel);
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
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var result = await imageServices.UploadAsync(model.Image, imageName, "/Images/Books", true);

                if (result.IsUploaded)
                {
                    book.ImageUrl = $"/Images/Books/{imageName}";
                    book.ImageThumbnailUrl = $"/Images/Books/thumb/{imageName}";
                }
                else
                {
                    ModelState.AddModelError(nameof(Image), result.errorMessage!);
                    var viewModel = PopulateViewModel(model);
                    return View("Form", viewModel);     
                }


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

            book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            foreach (var item in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = item });

            context.Books.Add(book);
            context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.Id });
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

            var book = context.Books.Include(c => c.Categories).Include(c => c.BookCopies).SingleOrDefault(b => b.Id == model.Id);
            if (book is null)
                return NotFound();

            //  string ImagePuplicId = null;

            if (model.Image != null)
            {
                if (book.ImageUrl != null)
                {
                    imageServices.Delete(book.ImageUrl, book.ImageThumbnailUrl); 

                    //await cloudinary.DeleteResourcesAsync(book.ImagePuplicId);
                }

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var result = await imageServices.UploadAsync(model.Image, imageName, "/Images/Books", true);

                if (result.IsUploaded)
                {
                    model.ImageUrl = $"/Images/Books/{imageName}";
                    model.ImageThumbnailUrl = $"/Images/Books/thumb/{imageName}";
                }
                else
                {
                    ModelState.AddModelError(nameof(Image), result.errorMessage!);
                    var viewModel = PopulateViewModel(model);
                    return View("Form", viewModel);
                }

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

            else if (model.ImageUrl is null)
            {
                model.ImageUrl = book.ImageUrl;
                model.ImageThumbnailUrl = book.ImageThumbnailUrl;
            }

            book = mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            //  book.ImagePuplicId = ImagePuplicId;
            foreach (var item in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = item });

            if (!model.IsAvilableForRental)
            {
                foreach (var item in book.BookCopies)
                {
                    item.IsAvilableForRental = false;
                }
            }

            context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }
        public IActionResult AllowBook(BookFormViewModel viewModel)
        {
            var book = context.Books.FirstOrDefault(b => b.Title == viewModel.Title && b.AuthorId == viewModel.AuthorId);
            var isAllow = book is null || book.Id.Equals(viewModel.Id);
            return Json(isAllow);

        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var book = context.Books.Find(id);
            if (book == null)
                return NotFound();
            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdatedId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            context.SaveChanges();
            return Ok(book.LastUpdatedOn.ToString());
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
