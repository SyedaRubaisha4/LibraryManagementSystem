
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Models.DBModel
{
    public class BookCategories
    {
        [Key]
        public int Id { get; set; }
        public int BookId { get; set; }
        public virtual Book Book { get; set; }

        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}
