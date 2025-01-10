using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using Models.DBModel;
using Models.DTOModel.JWT;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Scripting;
using Models.DTOModel.Users;
using Models.DTOModel;
using Models.DTOModel.Login;
using Microsoft.AspNetCore.Authorization;


namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenHelper _jwtTokenHelper;

        public AuthController(ApplicationDbContext context, JwtTokenHelper jwtTokenHelper)
        {
            _context = context;
            _jwtTokenHelper = jwtTokenHelper;
        }

        // 1. User Signup
        [HttpPost("CreateUser")]
        [Authorize]
        public async Task<IActionResult> CreateUser(string FirstName, string LastName, string Age, string Gender, double? Latitude, double? Longitude, string UniVersityName, string UniversityAddress, DateTime DateofBirth, string Password, string Email, IFormFile? Image)
        {
            if (await _context.User.AnyAsync(u => u.Email == Email))
                return BadRequest("Email already exists.");
            Password = BCrypt.Net.BCrypt.HashPassword(Password);
            var User = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Age = Age,
                Gender = Gender,
                Password = Password,
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
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
                Directory.CreateDirectory(uploadFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Image.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                   Image.CopyTo(fileStream);
                }

                User.ProfileImage = uniqueFileName;
            }
               _context.User.Add(User);
            await _context.SaveChangesAsync();

            return Ok("User created successfully.");
        }

        [HttpPost("login")]
      
        public async Task<IActionResult> login([FromBody] LoginRequest model)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == model.Email);
          
            //if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            //    return Unauthorized("Invalid credentials.");


            var accessToken = _jwtTokenHelper.GenerateToken(user);

            var refreshToken = GenerateRefreshToken();
            user.ResetToken = refreshToken;
            user.TokenExpiry = DateTime.UtcNow.AddDays(7); 
            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

       [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.ResetToken == model.RefreshToken);

            if (user == null || user.TokenExpiry < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token.");

            var newAccessToken = _jwtTokenHelper.GenerateToken(user);

          
            var newRefreshToken = GenerateRefreshToken();
            user.ResetToken = newRefreshToken;
            user.TokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

      
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }

   
   

    
}
