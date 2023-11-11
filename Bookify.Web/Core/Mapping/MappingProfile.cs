using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookify.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category
            CreateMap<Category, CategoryViewModel>();
            CreateMap<Category, CategoryFormViewModel>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember => sourceMember.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(sourceMember => sourceMember.Name));


            // Author
            CreateMap<Author, AuthorViewModel>();
            CreateMap<Author, AuthorFormViewModel>().ReverseMap();
            CreateMap<Author, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember => sourceMember.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(sourceMember => sourceMember.Name));

            // Book
            CreateMap<BookFormViewModel, Book>()
                .ReverseMap()
                .ForMember(dest => dest.Categories, opt => opt.Ignore());
            CreateMap<Book, BookViewModel>()
              .ForMember(dest => dest.Author, opt => opt.MapFrom(sourceMember => sourceMember.Author!.Name))
              .ForMember(dest => dest.Categories
              , opt => opt.MapFrom(sourceMember => sourceMember.Categories.Select(c => c.Category!.Name).ToList()));

            // Book Copies
            CreateMap<BookCopy, BookCopyViewModel>()
             .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(sourceMember => sourceMember.Book!.Title));
            CreateMap<BookCopy,BookCopyFormViewModel>().ReverseMap();

            // User
            CreateMap<ApplicationUsers, UsersViewModel>();
            CreateMap<UsersFormViewModel,ApplicationUsers>()
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(sourceMember => sourceMember.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(sourceMember => sourceMember.UserName.ToUpper()))

                .ReverseMap();
        }
    }
}
