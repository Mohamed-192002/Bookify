namespace Bookify.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUsers>
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BookCategory>().HasKey(x => new { x.CategoryId, x.BookId });

            builder.HasSequence<int>(name: "SerialNumber", schema: "shared").StartsAt(1000001);
            builder.Entity<BookCopy>()
                .Property(c => c.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            base.OnModelCreating(builder);
        }
    }
}