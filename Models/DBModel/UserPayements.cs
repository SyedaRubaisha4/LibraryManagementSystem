using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DBModel
{
    public class UserPayment
    {
        public int Id { get; set; }
        public string PaymentIntentId { get; set; }
        public int UserBookId { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public ReservedBooks UserBook { get; set; }
        public decimal AdminProfit { get; set; }
    }

}
