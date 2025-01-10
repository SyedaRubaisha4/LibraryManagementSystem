using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models.DTOModel.Users
{
    public class UpdateUserDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Email { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public bool DelImage {  get; set; }

        [Required]
        public string Roll { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string Age { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public string UniversityName { get; set; }
        [Required]
        public string UniversityAddress { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }

    }
}
