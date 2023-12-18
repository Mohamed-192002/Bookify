using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModel
{
    public class SubscriperFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;
        [Display(Name = "Date of birth")]
        [AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.AllowDate)]
        public DateTime DateOfBirth { get; set; }=DateTime.MinValue.AddYears(1950);
        [MaxLength(14, ErrorMessage = Errors.Length)]
        [MinLength(14, ErrorMessage = Errors.Length)]
        [Display(Name = "National ID")]
        [Remote("AllowNationalId", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string NationalId { get; set; } = null!;
        [MaxLength(15, ErrorMessage = Errors.MaxLength)]
        [Display(Name = "Mobile Number")]
        [RegularExpression(RegexPatterns.phoneRegex,ErrorMessage =Errors.RegexPhoneNumber)]
        [Remote("AllowPhoneNumber", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string MobileNumber { get; set; } = null!;
        [Display(Name = "Has WhatsApp?")]
        public bool HasWhatsApp { get; set; }
        [MaxLength(150, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowEmail", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Email { get; set; } = null!;
        [RequiredIf("Id==0", ErrorMessage = Errors.RequiredField)]
        public IFormFile? Image { get; set; }=null!;
        public string? ImageUrl { get; set; }
        public string? ImageThumbnailUrl { get; set; }
        [MaxLength(500)]
        public string Address { get; set; } = null!;
        public bool IsBlackListed { get; set; }
        [Display(Name ="Area")]
        public int AreaId { get; set; }
        public IEnumerable<SelectListItem>? Areas { get; set; }=new List<SelectListItem>();
        [Display(Name = "Governorate")]
        public int GovernorateId { get; set; }
        public IEnumerable<SelectListItem>? Governorates { get; set; }
    }
}
