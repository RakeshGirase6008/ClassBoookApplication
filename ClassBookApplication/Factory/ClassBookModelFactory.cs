using ClassBookApplication.Domain.Common;
using ClassBookApplication.Utility;

namespace ClassBookApplication.Factory
{
    public class ClassBookModelFactory
    {
        public object PrepareUserDetail(Users user)
        {
            return new
            {
                UserId = user.UserId,
                Email = user.Email,
                AuthorizeTokenKey = user.AuthorizeTokenKey
            };
        }
        public string PrepareImageUrl(string ImageUrl)
        {
            if (!string.IsNullOrEmpty(ImageUrl))
                return ClassBookConstant.WebSite_HostURL.ToString() + "/" + ImageUrl.Replace("\\", "/");
            return string.Empty;
        }
    }
}
