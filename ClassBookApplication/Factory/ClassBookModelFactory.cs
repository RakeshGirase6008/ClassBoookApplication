using ClassBookApplication.Domain.Common;

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
    }
}
