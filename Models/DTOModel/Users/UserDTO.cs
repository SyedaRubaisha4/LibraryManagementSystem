using System.ComponentModel.DataAnnotations;

namespace Models.DTOModel.Users
{
    public class UserDTO
    {


        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public string Email { get; set; }


        public IFormFile? Image { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string Age { get; set; }
        [Required]
        public string Gender { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? UniversityName { get; set; }
        public string? UniversityAddress { get; set; }


    }
}
