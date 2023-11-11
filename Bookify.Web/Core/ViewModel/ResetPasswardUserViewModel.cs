namespace Bookify.Web.Core.ViewModel
{
    public class ResetPasswardUserViewModel
    {
        public string? Id { get; set; }

        [StringLength(100, ErrorMessage = Errors.MaxMinLengh, MinimumLength = 6)]
        [RegularExpression(RegexPatterns.Passward, ErrorMessage = Errors.RegexPassword)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = Errors.ConfirmPassward)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
