using AutoMapper;
using LibraryManagementSystem.API.Helpers;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DBModel;
using Models.DTOModel.Book;
using Models.DTOModel;
using Models.DTOModel.Users;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SixLabors.ImageSharp;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public UserController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _mapper = mapper;
        }
        [HttpGet("GetAllUsers")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.User.ToListAsync();
                var baseUrl = $"{Request.Scheme}://{Request.Host.Value}/wwwroot/UserImages/";
                var getUser = users.Select(user => new GetUserDTO
                {
                    Id=user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Status = user.Status,
                    Roll = user.Roll,
                    Password = user.Password,
                    DateOfBirth = user.DateOfBirth,
                    UserAddedDate = user.UserAddedDate,
                    Age = user.Age,
                    Gender = user.Gender,
                    Latitude = user.Latitude,
                    Longitude = user.Longitude,
                    ResetToken = user.ResetToken,
                    TokenExpiry = user.TokenExpiry,
                    ProfileImage = baseUrl + user.ProfileImage,
                    UniversityName = user.UniversityName,
                    UniversityAddress = user.UniversityAddress
                }).ToList();

                return Ok(getUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
        [HttpGet("GetUserById/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(b => b.Id == id);
                if (user == null)
                {
                    return NotFound(
                        new
                        {
                            Message = $"User with id {id} not found"
                        }
                        );
                }
                var userDtos=_mapper.Map<GetUserDTO>(user);
                return Ok(userDtos);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
        [HttpPost("CreateUser")]
        [Authorize]
        public async Task<IActionResult> CreateUser(string FirstName, string LastName, string Age, string Gender, double? Latitude, double? Longitude, string UniVersityName, string UniversityAddress, DateTime DateofBirth, string Password, string Email, IFormFile? Image)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
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
                   
                    User.ProfileImage =await FileHelper.SaveFileAsync(Image, "UserImages");

                }
                _context.User.Add(User);
                await _context.SaveChangesAsync();

                

                return CreatedAtAction(nameof(GetUserById), new { id = User.Id }, User);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
        [HttpPut("UpdateUser/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id ,UpdateUserDTO upadateUserDto)
        {
            var user= await _context.User.FirstOrDefaultAsync(b => b.Id == id);
            if (user == null)
                return NotFound(new { Message = $"User with id {id} not found" });

            user.FirstName = upadateUserDto.FirstName;
            user.LastName= upadateUserDto.LastName;
            user.Age=upadateUserDto.Age;
            user.Gender = upadateUserDto.Gender;
            user.Status=upadateUserDto.Status;
            user.Roll=upadateUserDto.Roll;
            user.Password=upadateUserDto.Password;
            user.Latitude = upadateUserDto.Latitude;
            user.Longitude=upadateUserDto.Longitude;
            user.Email = upadateUserDto.Email;
            user.UniversityAddress = upadateUserDto.UniversityAddress;
            user.UniversityName = upadateUserDto.UniversityName;
            if(upadateUserDto.DelImage!=false)
            {
                user.ProfileImage = "";
            }
            else
            {
              
                if (upadateUserDto.Image != null && upadateUserDto.Image.Length > 0)
                {
                   

                    FileHelper.DeleteFile(user.ProfileImage);

                    user.ProfileImage = await FileHelper.SaveFileAsync(upadateUserDto.Image, "UserImages");

                }
            }
            user.ResetToken = user.ResetToken;
            user.TokenExpiry= user.TokenExpiry;
            _context.User.Update(user);
            await _context.SaveChangesAsync();
            return Ok("User updated");
        }
        [HttpPatch("PatchUser/{id}")]
        [Authorize]
        public async Task<IActionResult> PatchUser(int id, PatchUserDTO userpatchDto)
        {
            var user = await _context.User.FirstOrDefaultAsync(b => b.Id == id);
            if (user == null)
                return NotFound(new { Message = $"user with id {id} not found" });
       
            if(userpatchDto.FirstName!=null)  user.FirstName = userpatchDto.FirstName;
            if(userpatchDto.LastName!=null)   user.LastName = userpatchDto.LastName;
            if (userpatchDto.Age != null) user.Age = userpatchDto.Age;
            if (userpatchDto.Gender != null) user.Gender = userpatchDto.Gender;
            if (userpatchDto.Status != null) user.Status = userpatchDto.Status;
            if (userpatchDto.Roll != null) user.Roll = userpatchDto.Roll;
            if (userpatchDto.Password != null) user.Password = userpatchDto.Password;
            if (userpatchDto.Latitude != null) user.Latitude = userpatchDto.Latitude;
            if (userpatchDto.Longitude != null) user.Longitude = userpatchDto.Longitude;
            if (userpatchDto.Email != null) user.Email = userpatchDto.Email;
            if (userpatchDto.UniversityAddress != null) user.UniversityAddress = userpatchDto.UniversityAddress;
            if (user.UniversityName != null) user.UniversityName = userpatchDto.UniversityName;

           if(userpatchDto.DelImage==true)
            {
                user.ProfileImage = "";
            }
            else
            {
                if (userpatchDto.Image != null && userpatchDto.Image.Length > 0)
                {
                  
                    FileHelper.DeleteFile(user.ProfileImage);

                    user.ProfileImage = await FileHelper.SaveFileAsync(userpatchDto.Image, "UserImages");

                }
            }
           

            user.ResetToken = user.ResetToken;
            user.TokenExpiry = user.TokenExpiry;
            _context.User.Update(user);
            await _context.SaveChangesAsync();


            return Ok("User Updated  Successfully");
        }
        [HttpDelete("DeleteUserById/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(b => b.Id == id);
                if (user == null)
                {
                    return NotFound(
                    new
                    {
                        Message = $"user with id {id} not found"
                    });
                }

                user.Status = Status.Blocked.ToString();
                _context.User.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "User deleted successfully" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
