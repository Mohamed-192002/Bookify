namespace Bookify.Web.Core.ViewModel
{
    public class BookCopyFormViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        [Display(Name ="Is avilable for rental ?")]
        public bool IsAvilableForRental { get; set; }
        [Range(1, 1000,ErrorMessage = "Edition number should be less than 1000")]
        [Display(Name = "Edition number")]
        public int EditionNumber { get; set; }
        public bool ShowAvaliableButton { get; set; }
    }
}
