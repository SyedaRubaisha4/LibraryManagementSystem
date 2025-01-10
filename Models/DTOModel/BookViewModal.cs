namespace Models.DTOModel
{
    public class BookViewModal
    {
        public IEnumerable<Models.DBModel.Book> Books { get; set; }
        public Models.DBModel.Book Book { get; set; }

    }
}
