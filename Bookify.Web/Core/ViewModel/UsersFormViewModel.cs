using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModel
{

    public class UsersFormViewModel
    {
        public string? Id { get; set; }
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;
        [MaxLength(20, ErrorMessage = Errors.MaxLength), Display(Name = "User Name")]
        [Remote("AllowUserName", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string UserName { get; set; } = null!;
        [EmailAddress]
        [MaxLength(200, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowEmail", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;

        [StringLength(100, ErrorMessage = Errors.MaxMinLengh, MinimumLength = 6)]
        [RegularExpression(RegexPatterns.Passward, ErrorMessage = Errors.RegexPassword)]
        [DataType(DataType.Password)]
        [RequiredIf("Id==null",ErrorMessage =Errors.RequiredField)]
        public string? Password { get; set; }= null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = Errors.ConfirmPassward)]
        [RequiredIf("Id==null", ErrorMessage = Errors.RequiredField)]

        public string? ConfirmPassword { get; set; }=null!;

        public IList<string> SelectedRoles { get; set; } = new List<string>();
        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
