using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.DBModel;
using Models.DTOModel;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private readonly PasswordHasher<User> _passwordHasher;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext applicationDb, IConfiguration configuration, EmailService emailService)
        {
            _logger = logger;
            _context = applicationDb;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
            _emailService = emailService;
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.User.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["accountError"] = "No account found with this email.";
                return RedirectToAction("ForgotPassword");
            }

            var resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.TokenExpiry = DateTime.Now.AddHours(24);


            _context.User.Update(user);
            _context.SaveChangesAsync();

            var resetLink = Url.Action("ResetPassword", "Home", new { token = resetToken }, Request.Scheme);
            SendResetEmail(user.Email, resetLink);

            TempData["PasswordSuccess"] = "Password reset link has been sent to your email.";
            return RedirectToAction("ForgotPassword");
        }

        private void SendResetEmail(string userEmail, string resetLink)
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
                Subject = "Password Reset Request",
                Body = $"Click <a href='{resetLink}'>here</a> to reset your password.",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(userEmail);

            smtpClient.Send(mailMessage);
        }


        public IActionResult ResetPassword(string token)
        {
            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["DataError"] = "Invalid data submitted.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Token))
            {
                TempData["TokenError"] = "Token is missing.";
                return RedirectToAction("Login");
            }

            var user = _context.User.FirstOrDefault(u =>
                u.ResetToken == model.Token &&
                u.TokenExpiry.HasValue &&
                u.TokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                TempData["InvalidtokenError"] = "Invalid token or token has expired.";
                return RedirectToAction("Login");
            }

            user.Password = model.NewPassword;
            user.ResetToken = "";
            user.TokenExpiry = null;
            _context.SaveChangesAsync();

            TempData["PasSuccess"] = "Your password has been successfully reset!";
            return RedirectToAction("Login");
        }

        public IActionResult Index()
        {
            return RedirectToAction("ViewBook", "Book");
        }
        public IActionResult Logouts()
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("Password");
            return RedirectToAction("login", "Home");

        }

        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            var user = _context.User.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetString("Password", user.Password);


                if (user.Roll == Roll.Admin.ToString())
                {
                    var users = _context.User.Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName
                    }).ToList();

                    ViewData["Users"] = users;

                    return RedirectToAction("ViewBook", "Book");
                }
                else if (user.Roll == Roll.User.ToString())
                {
                    return RedirectToAction("BookView", "Book");
                }
            }
            TempData["Error"] = "Invalid username or password";
            return RedirectToAction("login");
        }
        public IActionResult login()
        {
            return View();
        }
        public IActionResult signup()
        {
            return View();
        }
        [HttpPost]
        public IActionResult signup(string FirstName, string LastName, string Age, string Gender, double? Latitude, double? Longitude, string UniVersityName, string UniversityAddress, DateTime DateofBirth, string Password, string Email, IFormFile? Image)
        {
            var person = _context.User.FirstOrDefault(p => p.Email == Email);
            var today = DateTime.Today;
            var age = today.Year - DateofBirth.Year;

            if (age < 14)
            {
                TempData["Dob"] = "Date of birth should be greater than 14";
                return View(person);
            }


            var user = new User
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
                Latitude = Latitude,
                Longitude = Longitude,
                UniversityName = UniVersityName,
                UniversityAddress = UniversityAddress
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

                user.ProfileImage = $"{uniqueFileName}";
            }
            else
            {
                user.ProfileImage = null;
            }

            // Save the user to the database
            _context.User.Add(user);
            _context.SaveChanges(); return RedirectToAction("login");

            //else
            //{
            //    TempData["UserExists"] = "Already user exists with this mail";
            //    return View(person);
            //}
        }
        public IActionResult Books()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
