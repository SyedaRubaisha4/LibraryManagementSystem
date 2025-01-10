using System.ComponentModel.DataAnnotations;

namespace Models.DTOModel.Book
{
    public class UpdateBookDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Author { get; set; }
        public Status Status { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? PdfFile { get; set; }

    }
}
