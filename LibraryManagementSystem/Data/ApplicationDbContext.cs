
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Models.DTOModel;
using Models.DBModel;
namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BookCategories>()
        .HasKey(bc => bc.Id);

            modelBuilder.Entity<BookCategories>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(bc => bc.BookId);

            modelBuilder.Entity<BookCategories>()
                .HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId);
            // Foreign key in UserPayment

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
                .HasKey(ub => new {ub.BookId,ub.UserId });

            modelBuilder.Entity<UserBooks>()
                .HasOne(b => b.Book)
                .WithMany(b => b.UserBooks)
                .HasForeignKey(b => b.BookId);

            modelBuilder.Entity<UserBooks>().HasOne(b => b.User)   .WithMany(b=>b.UserBooks).HasForeignKey(b=>b.UserId); 

            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-5VHCIAH;Database=LibraryManagement;Trusted_Connection=True;TrustServerCertificate=Yes");
        }
        public DbSet<User> User { get; set; }
        public DbSet<Book> Book { get; set; }
        public DbSet<UserPayment> UserPayments { get; set; }
      //  public DbSet<UserBooks> UserBooks { get; set; }
        public DbSet<ReservedBooks> ReservedBook { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<BookCategories> BookCategory { get; set; }

    }
}
