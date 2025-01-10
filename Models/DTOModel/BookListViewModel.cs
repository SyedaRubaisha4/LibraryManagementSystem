using Models.DBModel;
namespace Models.DTOModel
{
    public class BookListViewModel
    {
        public List<Models.DBModel.Book> Books { get; set; }
        public List<User> Users { get; set; }
    }
}
