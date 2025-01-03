namespace Models.DTOModel
{
    public class PaymentInfoViewModel
    {
        public string Firstname { get; set; }
        public string BookName { get; set; }
        public string PaymentStatus { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AdminPayments { get; set; }
    }

}
