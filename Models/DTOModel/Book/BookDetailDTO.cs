using System.ComponentModel.DataAnnotations;

namespace Models.DTOModel.Book
{
    public class BookDetailDTO
    {

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Author { get; set; }

        public Status Status { get; set; }
        public DateOnly BookCreationDate { get; set; }

        public string? ImagePath { get; set; }
        public string? PdfFilePath { get; set; }

        [Required]
        public string QRCode { get; set; }
    }
}
