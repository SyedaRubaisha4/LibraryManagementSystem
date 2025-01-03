using System.ComponentModel.DataAnnotations;
namespace Models.DBModel
{
    public class UserBooks
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime BookDateTime { get; set; }

    }
}
