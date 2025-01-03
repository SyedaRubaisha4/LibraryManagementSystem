
using Models.DBModel;
namespace Models.DTOModel
{
    public class BookViewModal
    {
        public IEnumerable<Book> Books { get; set; }
        public Book Book { get; set; }
        
    }
}
