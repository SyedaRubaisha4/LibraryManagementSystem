namespace Models.DTOModel
{
    public class BookDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ProfileImage { get; set; }  // Assuming it's a file name or path to the image
    }
}
