using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DBModel
{
    public class FavoriteBook
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int BookId { get; set; }
        public Book Book { get; set; }
        [Range(1, 5)]
        public int? Rating { get; set; }
        public bool IsFavorite { get; set; }

        public DateTime BookFavoritedDate { get; set; } = DateTime.UtcNow;
    }
}
