using Microsoft.Data.SqlClient;
using System.Data;

namespace Models.DTOModel
{
    public class ReportsRepository
    {
        string connectionString = "Server=DESKTOP-5VHCIAH;Database=LibraryManagement;Trusted_Connection=True;TrustServerCertificate=True";

        public  async Task<(List<BookBorrowReportViewModel> data, int totalRecords)> GetBookBorrowReportWithPaginationAsync(DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now;
            var result = new List<BookBorrowReportViewModel>();
            int totalRecords = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetBooksMostBorrow", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });
                    command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                    command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                    command.Parameters.Add(new SqlParameter("@Borrow", SqlDbType.NVarChar) { Value = "DESC" });

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
                        if (await reader.NextResultAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                            }
                        }
                    }
                }
            }

            return (result, totalRecords);
        }

        public async Task<(List<BookBorrowReportViewModel> data, int totalRecords)> UserActivity(DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {

            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now;

            var result = new List<BookBorrowReportViewModel>();
            int totalRecords = 0;

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetBooksMostBorrow", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });
                    command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                    command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                    command.Parameters.Add(new SqlParameter("@Borrow", SqlDbType.NVarChar) { Value = "ASC" });

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

                        if (await reader.NextResultAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                            }
                        }
                    }
                }
            }

            return (result, totalRecords);
        }
        public (List<UserReportViewModel> data, int TotalRecords) ActiveUsers(DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now;
            var result = new List<UserReportViewModel>();

            int totalRecords = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("p", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });
                    command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                    command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                    command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = "Active" });

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var users = new UserReportViewModel
                            {
                                UserName = reader["UserName"].ToString(),
                                TotalBooksBorrowed = Convert.ToInt32(reader["TotalBooksBorrowed"]),
                                LastBorrowedDate = reader["LastBorrowedDate"] as DateTime?,
                                BookTitles = reader["BookTitles"].ToString() // Capture the aggregated book titles
                            };
                            result.Add(users);
                        }

                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                            }
                        }
                    }
                }
            }
            return (result, totalRecords);
        }

        public (List<UserReportViewModel> data, int TotalRecords) InActiveUsers(DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now;
            var result = new List<UserReportViewModel>();

            int totalRecords = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("p", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });
                    command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                    command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                    command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = "Inactive" });

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var users = new UserReportViewModel
                            {
                                UserName = reader["UserName"].ToString(),
                                TotalBooksBorrowed = Convert.ToInt32(reader["TotalBooksBorrowed"]),
                                LastBorrowedDate = reader["LastBorrowedDate"] as DateTime?,
                                BookTitles = reader["BookTitles"].ToString() // Capture the aggregated book titles
                            };
                            result.Add(users);
                        }

                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                            }
                        }
                    }
                }
            }
            return (result, totalRecords);
        }

        public (List<FineReportViewModel> data, int totalRecords, decimal totalFineAmount) Fine(DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
          
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now;
            var result = new List<FineReportViewModel>();

            int totalRecords = 0;
            decimal totalFineAmount = 0;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("FineReport", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });
                    command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                    command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                    using (var reader = command.ExecuteReader())
                    {
                        // First result set: Detailed fine report
                        while (reader.Read())
                        {
                            var users = new FineReportViewModel
                            {
                                UserName = reader["UserName"].ToString(),
                                BookTitle = reader["BookTitle"].ToString(),
                                PaymentStatus = reader["PaymentStatus"].ToString(),
                                FineAmount = Convert.ToDecimal(reader["FineAmount"])
                            };
                            result.Add(users);

                            // Add FineAmount to the total
                            totalFineAmount += users.FineAmount;
                        }

                        // Move to the next result set (for total fine amount)
                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                totalFineAmount = Convert.ToDecimal(reader["TotalFineAmount"]);
                            }
                        }

                        // Move to the next result set (for total records)
                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                            }
                        }
                    }
                }
            }

            return (result, totalRecords, totalFineAmount);
        }

        public (List<UserReportViewModel> data, int TotalRecords) one(DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now;
            var result = new List<UserReportViewModel>();

            int totalRecords = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("p", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.DateTime) { Value = startDate });
                    command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.DateTime) { Value = endDate });
                    command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                    command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });
                       command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar) { Value = "Inactive" });

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var users = new UserReportViewModel
                            {
                                UserName = reader["UserName"].ToString(),
                                TotalBooksBorrowed = Convert.ToInt32(reader["TotalBooksBorrowed"]),
                                LastBorrowedDate = reader["LastBorrowedDate"] as DateTime?,
                                 BookTitles = reader["BookTitles"].ToString() 
                            };
                            result.Add(users);
                        }

                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                            }
                        }
                    }
                }
            }
            return (result, totalRecords);
        }

    }
}
