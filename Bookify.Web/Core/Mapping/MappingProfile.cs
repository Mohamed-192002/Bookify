namespace Bookify.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category
            CreateMap<Category, CategoryViewModel>();
            CreateMap<Category, CategoryFormViewModel>().ReverseMap();

            // Author
            CreateMap<Author, AuthorViewModel>();
            CreateMap<Author, AuthorFormViewModel>().ReverseMap();
        }
    }
}
