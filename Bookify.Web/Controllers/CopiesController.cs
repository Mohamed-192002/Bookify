using Bookify.Web.Core.Models;
using Bookify.Web.Settings;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CopiesController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        public CopiesController(ApplicationDbContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = context.Books.Find(bookId);
            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                BookId = bookId,
                ShowAvaliableButton = book.IsAvilableForRental
            };
            return PartialView("Form", viewModel);
        }
        [HttpPost]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = context.Books.Find(model.BookId);
            if (book is null)
                return NotFound();

            var copy = new BookCopy
            {
                EditionNumber = model.EditionNumber,
                IsAvilableForRental = book.IsAvilableForRental && model.IsAvilableForRental,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };
            book.BookCopies.Add(copy);
            context.SaveChanges();

            var copyViewModel = mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRaw", copyViewModel);
        }
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = context.BookCopies.Include(b => b.Book).SingleOrDefault(c => c.Id == id);
            if (copy is null)
                return NotFound();
            var copyViewModel = mapper.Map<BookCopyFormViewModel>(copy);

            copyViewModel.ShowAvaliableButton = copy.Book!.IsAvilableForRental;

            return PartialView("Form", copyViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = context.BookCopies.Include(b => b.Book).SingleOrDefault(c => c.Id == model.Id);
            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvilableForRental = copy.Book!.IsAvilableForRental && model.IsAvilableForRental;
            copy.LastUpdatedId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            context.SaveChanges();

            var copyViewModel = mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRaw", copyViewModel);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = context.BookCopies.Find(id);
            if (copy == null)
                return NotFound();
            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdatedId= User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            context.SaveChanges();
            return Ok();
        }
    }
}
