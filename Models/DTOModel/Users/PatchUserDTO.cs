using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models.DTOModel.Users
{
    public class PatchUserDTO
    {
        public string? FirstName { get; set; }
         
            public bool DelImage { get; set; }

        public string? LastName { get; set; }
        public string? Email { get; set; }
       
     
        public string? Status { get; set; }

           
        public string? Roll { get; set; }
           
        public string? Password { get; set; }
           
        public DateTime DateOfBirth { get; set; }
           
        public string? Age { get; set; }
           
        public string? Gender { get; set; }
           
        public double? Latitude { get; set; }
           
        public double? Longitude { get; set; }
           
        public string? UniversityName { get; set; }
           
        public string? UniversityAddress { get; set; }
    
        public IFormFile? Image { get; set; }

    }
}
