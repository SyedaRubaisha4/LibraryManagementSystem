﻿using System.ComponentModel.DataAnnotations;

namespace Models.DTOModel.Users
{
    public class GetUserDTO
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string Email { get; set; }
        [Required]
        public string Status { get; set; }

        [Required]
        public string Roll { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public DateTime UserAddedDate { get; set; }
        [Required]
        public string Age { get; set; }
        [Required]
        public string Gender { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string ResetToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public string? ProfileImage { get; set; }
        public string? UniversityName { get; set; }
        public string? UniversityAddress { get; set; }



    }
}
