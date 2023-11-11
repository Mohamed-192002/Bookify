using Bookify.Web.Core.Models;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Security.Claims;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUsers> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IMapper mapper;
        public UsersController(UserManager<ApplicationUsers> _userManager, RoleManager<IdentityRole> _roleManager, IMapper _mapper)
        {
            userManager = _userManager;
            roleManager = _roleManager;
            mapper = _mapper;
        }
        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();
            var viewModel = mapper.Map<IEnumerable<UsersViewModel>>(users);
            return View(viewModel);
        }

        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var viewModel = new UsersFormViewModel()
            {
                Roles = await roleManager.Roles.Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name
                }).ToListAsync(),
            };
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsersFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            ApplicationUsers user = new()
            {
                UserName = viewModel.UserName,
                FullName = viewModel.FullName,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                Email = viewModel.Email,
                EmailConfirmed=true
            };
            var result = await userManager.CreateAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(user, viewModel.SelectedRoles);
                var model = mapper.Map<UsersViewModel>(user);
                return PartialView("_UserRow", model);
            }
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }

        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var viewModel = mapper.Map<UsersFormViewModel>(user);
            viewModel.SelectedRoles = await userManager.GetRolesAsync(user);
            viewModel.Roles = await roleManager.Roles.Select(r => new SelectListItem
            {
                Text = r.Name,
                Value = r.Name
            }).ToListAsync();
            return PartialView("_Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsersFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await userManager.FindByIdAsync(viewModel.Id);
            if (user == null)
                return NotFound();

            var oldRole = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, oldRole);

            user = mapper.Map(viewModel, user);
            user.LastUpdatedId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;
            await userManager.AddToRolesAsync(user, viewModel.SelectedRoles);

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var model = mapper.Map<UsersViewModel>(user);
                return PartialView("_UserRow", model);
            }
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdatedOn = DateTime.Now;
            await userManager.UpdateAsync(user);
            return Ok(user.LastUpdatedOn.ToString());
        }
        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            var viewModel = new ResetPasswardUserViewModel { Id = user.Id };
            return PartialView("_ResetPassword", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswardUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await userManager.FindByIdAsync(viewModel.Id);
            if (user is null)
                return NotFound();

            var currentPassword = user.PasswordHash;
            await userManager.RemovePasswordAsync(user);
            var result = await userManager.AddPasswordAsync(user, viewModel.Password);
            if (result.Succeeded)
            {
                user.LastUpdatedId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.LastUpdatedOn = DateTime.Now;
                await userManager.UpdateAsync(user);
                var model = mapper.Map<UsersViewModel>(user);
                return PartialView("_UserRow", model);
            }

            user.PasswordHash = currentPassword;
            await userManager.UpdateAsync(user);
            return BadRequest(string.Join(',', result.Errors.Select(error => error.Description)));

            //var hashPassword = userManager.PasswordHasher.HashPassword(user, viewModel.Password);
            //user.PasswordHash = hashPassword;
            //user.LastUpdatedOn = DateTime.Now;
            //await userManager.UpdateAsync(user);

            //var model = mapper.Map<UsersViewModel>(user);

            //return PartialView("_UserRow", model);
        }

        public async Task<IActionResult> AllowUserName(UsersFormViewModel viewModel)
        {
            var user = await userManager.FindByNameAsync(viewModel.UserName);
            var isAllow = user is null || user.Id.Equals(viewModel.Id);

            return Json(isAllow);
        }
        public async Task<IActionResult> AllowEmail(UsersFormViewModel viewModel)
        {
            var user = await userManager.FindByEmailAsync(viewModel.Email);
            var isAllow = user is null || user.Id.Equals(viewModel.Id);
            return Json(isAllow);
        }
        public IActionResult test()
        {
            return View(new BookFormViewModel());
        }
    }


}
