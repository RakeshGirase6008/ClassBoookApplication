namespace ClassBookApplication.Models.ResponseModel
{
    public class CartDetailModel
    {
        public string Board { get; set; }
        public string Medium { get; set; }
        public string Standard { get; set; }
        public string Subject { get; set; }
        public decimal Amount { get; set; }
    }

    public class ClassDetailModels
    {
        public string Board { get; set; }
        public int BoardId { get; set; }
        public string Medium { get; set; }
        public int MediumId { get; set; }
        public string Standard { get; set; }
        public int StandardsId { get; set; }
    }
}
