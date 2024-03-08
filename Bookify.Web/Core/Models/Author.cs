namespace Bookify.Web.Core.Models
{
    public class Author:BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

    }
}
