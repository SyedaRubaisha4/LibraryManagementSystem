namespace Models.DTOModel
{
    public class FavoriteBookDto
    {
        public int BookId { get; set; }
        public int Rating { get; set; } // 0 if no rating is submitted
    }
}
