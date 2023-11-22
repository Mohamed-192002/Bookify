using Bookify.Web.Core.Models;
using Bookify.Web.Services;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using System.Data;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace Bookify.Web.Controllers
{
	[Authorize(Roles = AppRoles.Admin)]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUsers> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IEmailSender emailSender;
		private readonly IEmailBodyBuilder emailBodyBuilder;

		private readonly IMapper mapper;
		public UsersController(UserManager<ApplicationUsers> _userManager, RoleManager<IdentityRole> _roleManager, IMapper _mapper, IEmailSender _emailSender, IEmailBodyBuilder emailBodyBuilder)
		{
			userManager = _userManager;
			roleManager = _roleManager;
			mapper = _mapper;
			emailSender = _emailSender;
			this.emailBodyBuilder = emailBodyBuilder;
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
				Email = viewModel.Email
			};
			var result = await userManager.CreateAsync(user, viewModel.Password!);

			if (result.Succeeded)
			{
				await userManager.AddToRolesAsync(user, viewModel.SelectedRoles);

				var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = Url.Page(
					"/Account/ConfirmEmail",
					pageHandler: null,
					values: new { area = "Identity", userId = user.Id, code },
					protocol: Request.Scheme);

				#region Send massage Email
				var body = emailBodyBuilder.GetEmailBody
					(
					"https://res.cloudinary.com/mhmdnosair/image/upload/v1700380565/icon-positive-vote-1_nvd6xb.png"
					, $"Hey {user.FullName}, thanks for joining us!", "please confirm your email"
					, $"{HtmlEncoder.Default.Encode(callbackUrl!)}"
					, "Active Acount"
					);
				await emailSender.SendEmailAsync(user.Email, "Confirm your email", body);
				#endregion
				 

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
				await userManager.UpdateSecurityStampAsync(user);

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

		public async Task<IActionResult> UnLock(string id)
		{
			var user = await userManager.FindByIdAsync(id);
			if (user is null)
				return NotFound();
			if (!await userManager.IsLockedOutAsync(user))
				return Ok();

			user.LockoutEnd = null;
			await userManager.UpdateAsync(user);

			return Ok();
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
