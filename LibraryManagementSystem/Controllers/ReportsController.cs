using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using System.Threading.Tasks;
using LibraryManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace LibraryManagementSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ReportsRepository _reportsRepository;
        public ReportsController(ApplicationDbContext context, ReportsRepository reportsRepository)
        {
            _context = context;
            _reportsRepository = reportsRepository;
        }
        public IActionResult FineReport(DateTime? startDate, DateTime? endDate,  int pageSize=10, int pageNumber = 1)
        {

            List<SelectListItem> options = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "5",
                    Text="5"
                },
                new SelectListItem()
                {
                    Value = "10",
                    Text="10"
                },
                new SelectListItem()
                {
                    Value = "15",
                    Text="15"
                }
            };

            ViewData["Options"] = options;

            var (data, totalRecords, totalFineAmount) = _reportsRepository.Fine(startDate, endDate, pageNumber, pageSize);
            var BookBorrowReportViewModel = new FinePagesViewModel
            {
                FineReports = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalFineAmount = totalFineAmount
            };
            return View(BookBorrowReportViewModel);
        }

        //[HttpGet]
        //public async Task<IActionResult> BookBorrowReportss(DateTime? startDate, DateTime? endDate)
        //{
        //    startDate = startDate ?? DateTime.Now.AddMonths(-1);
        //    endDate = endDate ?? DateTime.Now;

        //    var reportData = await _context.GetBookBorrowReportAsync(startDate, endDate);



        //    return View(reportData); 
        //}

        public async Task<IActionResult> BookBorrowReportss(DateTime? startDate, DateTime? endDate,int pageNumber=1, int pageSize=10)
        {
            //var userId = HttpContext.Session.GetInt32("UserId");
            List<SelectListItem> options = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "5",
                    Text="5"
                },
                new SelectListItem()
                {
                    Value = "10",
                    Text="10"
                },
                new SelectListItem()
                {
                    Value = "15",
                    Text="15"
                }
            };

            ViewData["Options"] = options;

            var (data, totalRecords) = await _reportsRepository.GetBookBorrowReportWithPaginationAsync(startDate, endDate, pageNumber, pageSize);
           
            

            var BookBorrowReportViewModel = new BookBorrowPagesViewModel { 
                Data=data,
                TotalRecords=totalRecords,
                PageNumber=pageNumber,
                PageSize=pageSize
            };

           return View (BookBorrowReportViewModel);
           

            //if (userId == null)
            //{
            //    TempData["ErrorMessage"] = "Unauthorized.";
            //    return RedirectToAction("Login", "Home");
            //}

            //var userRole = HttpContext.Session.GetString("UserRole");
            //if (userRole == "Admin")
            //{
            //    ViewBag.UserName = HttpContext.Session.GetString("UserName");
            //    ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            //    ViewBag.ProfileImage = HttpContext.Session.GetString("ProfileImage");
            //    ViewBag.UserRole = userRole;

            //try
            //              {
            //            }
            //          catch (Exception ex)
            //        {
            //TempData["ErrorMessage"] = $"Error generating report: {ex.Message}";
            //return RedirectToAction("Index", "Book");
            //      }
            // }
            //else
            //{
            //    TempData["ErrorMessage"] = "Unauthorized";
            //    return RedirectToAction("Index", "Book");
            //}
        }
        public async Task<IActionResult> BookBorrowLeastReportss(DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 3)
        {
            List<SelectListItem> options = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "5",
                    Text="5"
                },
                new SelectListItem()
                {
                    Value = "10",
                    Text="10"
                },
                new SelectListItem()
                {
                    Value = "15",
                    Text="15"
                }
            };

            ViewData["Options"] = options;

            var (data, totalRecords) = await _reportsRepository.UserActivity(startDate, endDate, pageNumber, pageSize);

            var BookBorrowReportViewModel = new BookBorrowPagesViewModel
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return View(BookBorrowReportViewModel);
        }

        public async Task<IActionResult> ActiveUsersReport(DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 4)
        {
            List<SelectListItem> options = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "5",
                    Text="5"
                },
                new SelectListItem()
                {
                    Value = "10",
                    Text="10"
                },
                new SelectListItem()
                {
                    Value = "15",
                    Text="15"
                }
            };

            ViewData["Options"] = options;

            var (data, totalRecords) =  _reportsRepository.ActiveUsers(startDate, endDate, pageNumber, pageSize);

            var AllUsers = new UserActivity { 
                UserReportViewModels = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
               
              
            };
            return View(AllUsers);

        }
        public async Task<IActionResult> InactiveUsersReport(DateTime? startDate, DateTime? endDate, int pageNumber = 1, int pageSize = 4)
        {
            List<SelectListItem> options = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Value = "5",
                    Text="5"
                },
                new SelectListItem()
                {
                    Value = "10",
                    Text="10"
                },
                new SelectListItem()
                {
                    Value = "15",
                    Text="15"
                }
            };

            ViewData["Options"] = options;

            var (data, totalRecords) = _reportsRepository.InActiveUsers(startDate, endDate, pageNumber, pageSize);

         
            var AllUsers = new UserActivity
            {
                UserReportViewModels = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
               ,

            };
            return View(AllUsers);
        }
      

    }

}
