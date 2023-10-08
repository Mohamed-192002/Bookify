namespace Bookify.Web.Core.ViewModel
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100),Display(Name ="Category Name"),Required]
        public string Name { get; set; } = string.Empty;
    }
}
