﻿using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
   [Authorize(Roles =AppRoles.Archive)]
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AuthorsController(ApplicationDbContext _context, IMapper _mapper)
        {
            context = _context;
            mapper = _mapper;
        }
        public IActionResult Index()
        {
            var authors = context.Authors.AsNoTracking().ToList();

            var viewModelMapping = mapper.Map<IEnumerable<AuthorViewModel>>(authors);
            return View(viewModelMapping);
        }
        [AjaxOnly]
        public IActionResult Create()
        {
            var viewModel = new AuthorFormViewModel();
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var model = mapper.Map<Author>(viewModel);
            model.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            context.Authors.Add(model);
            context.SaveChanges();

            var authorViewModel = mapper.Map<AuthorViewModel>(model);

            return PartialView("_AuthorRow", authorViewModel);
        }
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var author = context.Authors.Find(id);
            if (author == null)
                return NotFound();
            var viewModel = mapper.Map<AuthorFormViewModel>(author);
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AuthorFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var author = context.Authors.Find(viewModel.Id);
            author = mapper.Map(viewModel, author);
            author.LastUpdatedId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            author.LastUpdatedOn = DateTime.Now;
            context.SaveChanges();

            var authorViewModel = mapper.Map<AuthorViewModel>(author);

            return PartialView("_AuthorRow", authorViewModel);
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var author = context.Authors.Find(id);
            if (author == null)
                return NotFound();
            author.IsDeleted = !author.IsDeleted;
            author.LastUpdatedOn = DateTime.Now;
            author.LastUpdatedId= User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            context.SaveChanges();
            return Ok(author.LastUpdatedOn.ToString());
        }
        public IActionResult AllowAuthor(AuthorFormViewModel viewModel)
        {
            var author = context.Authors.FirstOrDefault(c => c.Name == viewModel.Name);
            var isAllow = author is null || author.Id.Equals(viewModel.Id);
            return Json(isAllow);
        }
    }
}
