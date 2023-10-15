namespace Bookify.Web.Core.ViewModel
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100),Display(Name ="Category Name"),Required]
        [Remote("AllowCategory", null , ErrorMessage ="This category is exist!")]
        public string Name { get; set; } = string.Empty;
    }
}
