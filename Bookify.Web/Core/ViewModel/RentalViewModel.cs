﻿namespace Bookify.Web.Core.ViewModel
{
    public class RentalViewModel
    {
        public int Id { get; set; }
        public SubscriperViewModel? Subscriper { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public bool PenaltyPaid { get; set; }
        public IEnumerable<RentalCopyViewModel> RentalCopies { get; set; } = new List<RentalCopyViewModel>();
        public int TotalDelayInDays
        {
            get
            {
                return RentalCopies.Sum(c => c.DelayInDays);
            }
        }
        public int NumberOfCopies
        {
            get
            {
                return RentalCopies.Count();
            }
        }
    }
}
