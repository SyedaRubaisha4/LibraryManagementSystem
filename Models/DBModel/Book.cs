using Models.DTOModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DBModel
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public Status Status { get; set; } = Status.Active;
        public string? ProfileImage { get; set; }
        public DateOnly BookCreationDate { get; set; }
        public string QRCode { get; set; }
        public double? AverageRating { get; set; }
        [Required]
        public DateTime BookAddedDate { get; set; }
        [NotMapped]
        public IFormFile? Image { get; set; }
        public string? PdfFileName { get; set; }
        [NotMapped]
        public IFormFile? PdfFile { get; set; }
        [NotMapped]
        public List<int> CategoryIds { get; set; }
        public virtual ICollection<UserBooks> UserBooks { get; set; }


    }
}
