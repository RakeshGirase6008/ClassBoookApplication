namespace ClassBookApplication.Models.ResponseModel
{
    public class CartDetailModel
    {
        public string Type { get; set; }
        public string ProviderName { get; set; }
        public string Board { get; set; }
        public string Medium { get; set; }
        public string Standard { get; set; }
        public string Subject { get; set; }
        public decimal Amount { get; set; }
    }
}
