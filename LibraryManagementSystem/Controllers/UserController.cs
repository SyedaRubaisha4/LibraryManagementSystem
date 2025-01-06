using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DBModel;
using Models.DTOModel;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
namespace LibraryManagementSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        public UserController(ApplicationDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            _context = context;
        }

        public IActionResult AddUser(int page = 1, int pageSize = 3, string searchQuery = "", string sortBy = "Id", bool isAscending = true, string universityFilter = "")
        {
            var query = _context.User.AsQueryable();
            var universityList = _context.User
      .Select(u => u.UniversityName)
      .Distinct()
      .ToList();
            ViewBag.UniversityList = universityList;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(u => u.FirstName.Contains(searchQuery) || u.LastName.Contains(searchQuery));
            }

            // Apply sorting
            query = sortBy switch
            {
                "FirstName" => isAscending ? query.OrderBy(u => u.FirstName) : query.OrderByDescending(u => u.FirstName),
                "LastName" => isAscending ? query.OrderBy(u => u.LastName) : query.OrderByDescending(u => u.LastName),
                "Age" => isAscending ? query.OrderBy(u => u.Age) : query.OrderByDescending(u => u.Age),
                _ => query.OrderByDescending(u => u.Id),
            };

            if (!string.IsNullOrEmpty(universityFilter))
            {
                query = query.Where(u => u.UniversityName == universityFilter);
            }

            var totalCount = query.Count();
            var users = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Users = users;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.TotalCount = totalCount;
            ViewBag.SortBy = sortBy;
            ViewBag.IsAscending = isAscending;

            var viewModel = new UserViewModel
            {
                Users = users,
                User = new User()
            };

            return View(viewModel);
        }
        [HttpGet]
        public JsonResult CheckEmail(string email)
        {
            var userExists = _context.User.Any(u => u.Email == email);
            return Json(new { exists = userExists });
        }
        [HttpPost]
        public IActionResult SendFine()
        {
            try
            {
                var usersToSendReminder = (from userBook in _context.ReservedBook
                                           join book in _context.Book on userBook.BookId equals book.Id
                                           join user in _context.User on userBook.UserId equals user.Id
                                           let daysReserved = EF.Functions.DateDiffDay(userBook.BookDateTime, DateTime.UtcNow)
                                           where daysReserved > 1
                                           group new
                                           {
                                               book.Name,
                                               userBook.BookDateTime,
                                               DaysReserved = daysReserved,
                                               Fine = daysReserved * 5
                                           } by new
                                           {
                                               user.FirstName,
                                               user.Email
                                           } into userGroup
                                           select new
                                           {
                                               FirstName = userGroup.Key.FirstName,
                                               Email = userGroup.Key.Email,
                                               Books = userGroup.ToList(),
                                               TotalFine = userGroup.Sum(b => b.Fine)
                                           }).ToList();

                foreach (var reminder in usersToSendReminder)
                {
                    var booksTableRows = string.Join("", reminder.Books.Select(book => $@"
            <tr>
                <td style='border: 1px solid black; text-align: left; padding: 8px;'>{book.Name}</td>
                <td style='border: 1px solid black; text-align: left; padding: 8px;'>{book.BookDateTime:yyyy-MM-dd}</td>
                <td style='border: 1px solid black; text-align: left; padding: 8px;'>{book.DaysReserved}</td>
                <td style='border: 1px solid black; text-align: left; padding: 8px;'>5</td>
                <td style='border: 1px solid black; text-align: left; padding: 8px;'>{book.Fine}</td>
            </tr>
        "));

                    SendEmail(
                        toEmail: reminder.Email,
                        subject: "Pay Your Fine",
                        body: $@"
<html>
<body>
    <p>Dear {reminder.FirstName},</p>
    <p>Please find below the details of your reserved books:</p>
    <table style='border-collapse: collapse; width: 100%;'>
        <tr>
            <th style='border: 1px solid black; text-align: left; padding: 8px; background-color: #f2f2f2;'>Book Title</th>
            <th style='border: 1px solid black; text-align: left; padding: 8px; background-color: #f2f2f2;'>Reservation Date</th>
            <th style='border: 1px solid black; text-align: left; padding: 8px; background-color: #f2f2f2;'>Days Reserved</th>
            <th style='border: 1px solid black; text-align: left; padding: 8px; background-color: #f2f2f2;'>Fines Per Day</th>
            <th style='border: 1px solid black; text-align: left; padding: 8px; background-color: #f2f2f2;'>Total Fine</th>
        </tr>
        {booksTableRows}
    </table>
    <p>Total Fine for all books: <b>${reminder.TotalFine}</b></p>
    <p>Please return your reserved books as soon as possible. Thank you!</p>
</body>
</html>"
                    );
                }

                return Json(new { message = "Emails sent successfully to users." });
            }
            catch (Exception ex)
            {
                return Json(new { message = $"An error occurred: {ex.Message}" });
            }

        }

        [HttpPost]
        public IActionResult SendRemainders()
        {
            try
            {
                var usersToSendReminder = (from userBook in _context.ReservedBook
                                           join book in _context.Book on userBook.BookId equals book.Id
                                           join user in _context.User on userBook.UserId equals user.Id
                                           where userBook.BookDateTime >= DateTime.UtcNow.AddDays(-2)
                                           select new
                                           {
                                               FirstName = user.FirstName,
                                               Email = user.Email,
                                               BookTitle = book.Name,
                                               userBook.BookDateTime
                                           }).ToList();

                foreach (var reminder in usersToSendReminder)
                {
                    SendEmail(
                        toEmail: reminder.Email,
                        subject: "Book Return Reminder",
                        body: $"Dear {reminder.FirstName}, please return your reserved book: '{reminder.BookTitle}'. Thank you!"
                    );
                }

                return Json(new { message = "Emails sent successfully to users " });
            }
            catch (Exception ex)
            {
                return Json(new { message = $"An error occurred: {ex.Message}" });
            }
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient(_configuration["EmailSettings:Host"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:FromEmail"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            smtpClient.Send(mailMessage);

        }
        [HttpPost]
        public IActionResult Add(string FirstName, string LastName, string Age, string Gender, DateTime DateofBirth, string Password, string Email, IFormFile? Image)
        {
            var user = _context.User.FirstOrDefault(u => u.Email == Email);
            if (user != null)
            {
                return Json(new { success = false, message = "Email already exists. Please enter a unique email." });
            }
            var users = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Age = Age,
                Gender = Gender,
                DateOfBirth = DateofBirth,
                Password = Password,
                Email = Email,
                UserAddedDate = DateTime.Now,
                Roll = Roll.User.ToString(),
                Status = UserStatus.Active.ToString(),
                ResetToken = "",
                TokenExpiry = null,
            };

            if (Image != null && Image.Length > 0)
            {
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                Directory.CreateDirectory(uploadFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Image.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Image.CopyTo(fileStream);
                }

                users.ProfileImage = $"{uniqueFileName}";
            }
            else
            {
                users.ProfileImage = "download.jpeg";
            }


            _context.User.Add(users);
            _context.SaveChanges();

            return RedirectToAction("AddUser");
        }
        public IActionResult Userinfo()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int ids = int.Parse(userId);
            var user = _context.User.Find(ids);
            return View(user);
        }
        [HttpGet]
        public IActionResult GetUserDetails(int id)
        {
            var user = _context.User.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var userDetails = new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                age = user.Age,
                dateOfBirth = user.DateOfBirth.ToString("yyyy-MM-dd"),
                gender = user.Gender,
                password = user.Password,
                profileImage = Url.Content("~/Image/" + user.ProfileImage),
                email = user.Email
            };


            return Json(userDetails, new JsonSerializerOptions { PropertyNamingPolicy = null });
        }
        [HttpPost]
        public IActionResult Del(int id)
        {
            var person = _context.User.Find(id);
            if (person != null)
            {
                person.Status = UserStatus.Blocked.ToString();
                Console.WriteLine(person.FirstName + " " + person.LastName + " ");
                _context.SaveChanges();
            }
            return RedirectToAction("AddUser");
        }
        public void DeleteFile(string fileName)
        {

            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");

            string filePath = Path.Combine(uploadFolder, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);

            }

        }

        [HttpPost]
        public IActionResult Update(string FirstName, string LastName, string Age, string Gender, DateTime DateofBirth, string Password, string Email, int Id, IFormFile? profileImage)
        {
            var user = _context.User.FirstOrDefault(u => u.Id == Id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            user.FirstName = FirstName;
            user.LastName = LastName;
            user.Age = Age;
            user.Gender = Gender;
            user.DateOfBirth = DateofBirth;
            user.Password = Password;
            user.Email = Email;
            user.UserAddedDate = DateTime.Now;
            string img = user.ProfileImage;
            if (img != null)
            {
                DeleteFile(img);
            }
            if (profileImage != null && profileImage.Length > 0)
            {
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Image");
                Directory.CreateDirectory(uploadFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profileImage.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(fileStream);
                }
                user.ProfileImage = uniqueFileName;
            }


            _context.SaveChanges();

            return RedirectToAction("AddUser");
        }
    }
}
