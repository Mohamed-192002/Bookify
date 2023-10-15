
namespace Bookify.Web.Core.ViewModel
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Category Name"), Required]
        [Remote("AllowCategory", null, ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; } = string.Empty;
    }
}
