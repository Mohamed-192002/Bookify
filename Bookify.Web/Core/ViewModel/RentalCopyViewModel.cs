namespace Bookify.Web.Core.ViewModel
{
    public class RentalCopyViewModel
    {
        public BookCopyViewModel? BookCopy { get; set; }
        public DateTime RenralDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime? ExtendDate { get; set; }
        public int DelayInDays
        {
            get
            {
                var delay = 0;
                if (ReturnDate.HasValue && ReturnDate.Value > EndDate)
                    delay = (int)(ReturnDate.Value - EndDate).TotalDays;
                else if (!ReturnDate.HasValue)
                    delay = (int)(DateTime.Today - EndDate).TotalDays;
                return delay;
            }
        }
    }
}
