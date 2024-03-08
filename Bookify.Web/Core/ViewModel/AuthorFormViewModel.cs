namespace Bookify.Web.Core.ViewModel
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Author Name"), Required]
        [Remote("AllowAuthor", null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; } = string.Empty;
    }
}
