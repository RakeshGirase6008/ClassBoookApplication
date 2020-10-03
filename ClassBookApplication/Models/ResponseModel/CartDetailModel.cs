using System.Collections.Generic;

namespace ClassBookApplication.Models.ResponseModel
{
    public class CartDetailModel
    {
        public string ProviderType { get; set; }
        public string LearningType { get; set; }
        public decimal ActualFees { get; set; }
        public string ProviderName { get; set; }
        public string BoardName { get; set; }
        public string MediumName { get; set; }
        public string StandardsName { get; set; }
        public string EnityName { get; set; }
        public string TypeOfMapping { get; set; }
    }
    public class SubscriptionDetailModel
    {
        public string ProviderType { get; set; }
        public string LearningType { get; set; }
        public decimal PaidAmount { get; set; }
        public string ProviderName { get; set; }
        public string BoardName { get; set; }
        public string MediumName { get; set; }
        public string StandardsName { get; set; }
        public string EnityName { get; set; }
        public string TypeOfMapping { get; set; }
        public string SubscriptionDate { get; set; }
        public string ExpireDate { get; set; }
    }


    public class TranscationDetailModel
    {
        public string ProviderType { get; set; }
        public string LearningType { get; set; }
        public decimal PaidAmount { get; set; }
        public string ProviderName { get; set; }
        public string EnityName { get; set; }
        public string TypeOfMapping { get; set; }
        public string OrderDate { get; set; }
        public string ExpireDate { get; set; }
    }

    public class CartCompleteDetail
    {
        public CartCompleteDetail()
        {
            CartDetailModel = new List<CartDetailModel>();
        }
        public decimal GrandTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal ClassBookHandlingAmount { get; set; }
        public decimal InternetHandlingCharge { get; set; }
        public decimal GST { get; set; }
        public List<CartDetailModel> CartDetailModel { get; set; }
        
    }
}
