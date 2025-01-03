namespace Models.DTOModel
{
    public class BookDetails
    {
        public int Id { get; set; }
        public string BookName { get; set; }
        public DateTime CreationDate { get; set; }
        public string AuthorName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateOnly BookCreationDate { get; set; }

        // Add the IsReserved property
        public bool IsReserved { get; set; } // Indicates if the book is reserved by the current user
    }
}
