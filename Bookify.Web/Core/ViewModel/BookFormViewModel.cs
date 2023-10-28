﻿using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModel
{
	public class BookFormViewModel
	{
		public int Id { get; set; }

		[MaxLength(500,ErrorMessage =Errors.MaxLength)]
        [Remote("AllowBook", null!,AdditionalFields = "Id,AuthorId", ErrorMessage = Errors.Duplicated)]
        public string Title { get; set; } = null!;

		[Display(Name ="Author")]
        [Remote("AllowBook", null!, AdditionalFields = "Id,Title", ErrorMessage = Errors.Duplicated)]
        public int AuthorId { get; set; }
		public IEnumerable<SelectListItem>? Authors { get; set; }
		[MaxLength(100, ErrorMessage = Errors.MaxLength)]
		public string Publisher { get; set; } = null!;
		[Display(Name = "Publisher Date")]
		[AssertThat("PublishingDate <= Today()",ErrorMessage =Errors.AllowDate)]
		public DateTime PublishingDate { get; set; }=DateTime.Now;
		public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }

        [MaxLength(50, ErrorMessage = Errors.MaxLength)]
		public string Hall { get; set; } = null!;
		[Display(Name = "is avilable for rental?")]
		public bool IsAvilableForRental { get; set; }
		public string Description { get; set; } = null!;
        [Display(Name = "Categories")]

        public IList<int> SelectedCategories { get; set; } = new List<int>();
		public IEnumerable<SelectListItem>? Categories { get; set; }

	}
}