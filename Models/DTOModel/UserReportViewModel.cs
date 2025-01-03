namespace Models.DTOModel
{
    public class UserReportViewModel
    {
        public string UserName { get; set; }
        public int TotalBooksBorrowed { get; set; }
        public DateTime? LastBorrowedDate { get; set; }
        public string BookTitles { get; set; } // Add this property
    }


}
