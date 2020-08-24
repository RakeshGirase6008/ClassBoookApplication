using ClassBookApplication.Domain.Common;

namespace ClassBookApplication.Factory
{
    public class ClassBookModelFactory
    {
        public object PrepareLoginUserDetail(Users user)
        {
            return new
            {
                UserId = user.UserId,
                Email = user.Email,
                DeviceTokenCode = user.DeviceTokenCode,
                AuthorizeTokenKey = user.AuthorizeTokenKey
            };
        }
    }
}
