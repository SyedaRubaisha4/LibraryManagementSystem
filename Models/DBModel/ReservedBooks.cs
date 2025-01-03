using Models.DBModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DBModel
{
    public class ReservedBooks
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime BookDateTime { get; set; }
        public ICollection<UserPayment> UserPayments { get; set; }

    }
}
