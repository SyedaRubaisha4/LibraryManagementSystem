
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.DBModel;
namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
         
            modelBuilder.Entity<UserPayment>()
                .HasOne(up => up.UserBook)               // Navigation property in UserPayment
                .WithMany(ub => ub.UserPayments)         // Navigation property in UserBooks
                .HasForeignKey(up => up.UserBookId);     // Foreign key in UserPayment
        
        modelBuilder.Entity<Book>(entity =>
            {
                entity.Property(e => e.Status)
                      .HasConversion<string>()  // Store the Status as string
                      .IsRequired();            // Make it required (non-nullable)
            });
            modelBuilder.Entity<User>(entity =>
            {
                // Configure the 'Status' and 'Roll' properties as strings
                entity.Property(e => e.Status)
                      .HasConversion<string>()  // Store as string
                      .IsRequired();            // Make it required

                entity.Property(e => e.Roll)
                      .HasConversion<string>()  // Store as string
                      .IsRequired();            // Make it required
            });
            modelBuilder.Entity<UserBooks>()
                .HasKey(ub => new { ub.BookId, ub.UserId });

            modelBuilder.Entity<UserBooks>()
                .HasOne(b => b.Book)
                .WithMany(b => b.UserBooks)
                .HasForeignKey(b => b.BookId);

            modelBuilder.Entity<UserBooks>().HasOne(b => b.User).WithMany(b => b.UserBooks).HasForeignKey(b => b.UserId);

            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        }
        public DbSet<User> User { get; set; }
        public DbSet<Book> Book { get; set; }
        public DbSet<UserPayment> UserPayments { get; set; }
        //  public DbSet<UserBooks> UserBooks { get; set; }
        public DbSet<ReservedBooks> ReservedBook { get; set; }

        public DbSet<FavoriteBook> FavoriteBooks { get; set; }
        public DbSet<Category> Category { get; set; }
       
    }
}
