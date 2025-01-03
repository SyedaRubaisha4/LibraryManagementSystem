
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
       
        public async Task<List<BookBorrowReportViewModel>> GetBookBorrowReportAsync(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now; 

            var result = new List<BookBorrowReportViewModel>();

            using (var connection = new SqlConnection("Server=DESKTOP-5VHCIAH;Database=LibraryManagement;Trusted_Connection=True;TrustServerCertificate=True"))
            {
                await connection.OpenAsync(); 

                using (var command = new SqlCommand("GetBooksMostBorrow", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    
                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });

                    using (var reader = await command.ExecuteReaderAsync()) 
                    {
                        while (await reader.ReadAsync())
                        {
                            var report = new BookBorrowReportViewModel
                            {
                                BookTitle = reader["BookTitle"].ToString(),
                                Author = reader["Author"].ToString(),
                                TotalBorrows = Convert.ToInt32(reader["TotalBorrows"]),
                                LastBorrowedDate = reader["LastBorrowedDate"] as DateTime?
                            };

                            result.Add(report);
                        }
                    }
                }
            }

            return result;
        }
        public async Task<List<BookBorrowReportViewModel>> GetBookLeastBorrowReportAsync(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);  
            endDate = endDate ?? DateTime.Now; 

            var result = new List<BookBorrowReportViewModel>();

            using (var connection = new SqlConnection("Server=DESKTOP-5VHCIAH;Database=LibraryManagement;Trusted_Connection=True;TrustServerCertificate=True"))
            {
                await connection.OpenAsync(); 

                using (var command = new SqlCommand("GetBooksLeastBorrow", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });

                    using (var reader = await command.ExecuteReaderAsync()) // Execute the stored procedure asynchronously
                    {
                        while (await reader.ReadAsync()) 
                        {
                            var report = new BookBorrowReportViewModel
                            {
                                 BookTitle = reader["BookTitle"].ToString(),
                                Author = reader["Author"].ToString(),
                                TotalBorrows = Convert.ToInt32(reader["TotalBorrows"]),
                                LastBorrowedDate = reader["LastBorrowedDate"] as DateTime? // Handle nullable dates
                            };

                            result.Add(report);
                        }
                    }
                }
            }

            return result;
        }

        //public async Task<List<BookBorrowReportViewModel>> GetBookLeastBorrowReportAsync(DateTime? startDate, DateTime? endDate)
        //{
        //    startDate = startDate ?? DateTime.Now.AddMonths(-1);  // Default to the last 30 days
        //    endDate = endDate ?? DateTime.Now;  // Default to the current date

        //    return await this.Database
        //        .SqlQueryRaw<BookBorrowReportViewModel>(
        //            "EXEC  GetBooksLeastBorrow  @StartDate={0}, @EndDate={1}",
        //            startDate, endDate)
        //        .ToListAsync();
        //}
        public async Task<List<UserReportViewModel>> GetActiveUserReportAsync(DateTime? StartDate, DateTime? EndDate)
        {
             StartDate = StartDate ?? DateTime.MinValue;  
            EndDate = EndDate ?? DateTime.MaxValue;      

             return await this.Database
                .SqlQueryRaw<UserReportViewModel>(
                    "EXEC  GetActiveUserReport @StartDate = {0}, @EndDate = {1}",
                    StartDate.Value, EndDate.Value)  
                .ToListAsync();
        }
        public async Task<List<ReminderViewModel>> GetUsersToSendReminderAsync(DateTime? startDate, DateTime? endDate)
        {
            var defaultStartDate = startDate ?? DateTime.UtcNow.AddDays(-30);
            var defaultEndDate = endDate ?? DateTime.UtcNow;

            return await this.Database
                .SqlQueryRaw<ReminderViewModel>(
                    "EXEC fineReport @StartDate = {0}, @EndDate = {1}",
                    defaultStartDate,
                    defaultEndDate)
                .ToListAsync();
        }

        public async Task<List<UserReportViewModel>> GetInactiveUsersReportAsync(DateTime? StartDate, DateTime? EndDate)
        {
             StartDate = StartDate ?? DateTime.MinValue; 
            EndDate = EndDate ?? DateTime.MaxValue;     

            return await this.Database
                .SqlQueryRaw<UserReportViewModel>(
                    "EXEC GetInactiveUsersReport @StartDate = {0}, @EndDate = {1}",
                    StartDate, EndDate)
                .ToListAsync();
        }
        public async Task<List<FineReportViewModel>> GetFineReportAsync(DateTime? startDate, DateTime? endDate)
        {
            startDate = startDate ?? DateTime.MinValue;
            endDate = endDate ?? DateTime.MaxValue;

            return await this.Database
                .SqlQueryRaw<FineReportViewModel>(
                    "EXEC FineReport @StartDate = {0}, @EndDate = {1}",
                    startDate , 
                    endDate ) 
                .ToListAsync();
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
      
    }
}
