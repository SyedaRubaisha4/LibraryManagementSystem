namespace Models.DTOModel
{
    public class BookBorrowReportViewModel
    {
        public string BookTitle { get; set; }
        public string Author { get; set; }
        public int TotalBorrows { get; set; }
        public DateTime? LastBorrowedDate { get; set; }
       
    }

}
