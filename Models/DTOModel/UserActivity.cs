namespace Models.DTOModel
{
    public class UserActivity
    {
        public List<UserReportViewModel>? UserReportViewModels { get; set; }
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
