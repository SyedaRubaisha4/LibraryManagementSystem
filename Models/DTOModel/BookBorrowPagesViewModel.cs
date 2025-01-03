﻿namespace Models.DTOModel
{
    public class BookBorrowPagesViewModel
    {
        public List<BookBorrowReportViewModel>? Data { get; set; }
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

}
