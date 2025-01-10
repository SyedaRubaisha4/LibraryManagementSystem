namespace Models.DTOModel.Book
{
    public class PatchBookDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Author { get; set; }
        public Status? Status { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? PdfFile { get; set; }
    }
}
