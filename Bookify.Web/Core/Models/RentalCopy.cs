using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Web.Core.Models
{
    public class RentalCopy
    {
        public int RentalId { get; set; }
        public Rental? Rental { get; set; }
        public int BookCopyId { get; set; }
        public BookCopy? BookCopy { get; set; }
        public DateTime RenralDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendDate { get; set; }


    }
}
