using ClassBookApplication.DataContext;
using ClassBookApplication.Models.PublicModel;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ClassBookApplication.Service
{
    public class MyAuthencationService 
    {
        #region Fields

        private readonly IConfiguration _config;
        private readonly ClassBookManagementContext _context;

        #endregion

        #region Ctor
        public MyAuthencationService(IConfiguration config,
            ClassBookManagementContext context)
        {
            this._config = config;
            this._context = context;
        }

        #endregion

        #region Method

        public UserLoginModel AuthenticateUser(UserLoginModel loginCredentials)
        {
            var user = from e in _context.Users
                       where e.Email == loginCredentials.Email && e.Password == loginCredentials.Password
                       select new UserLoginModel
                       {
                           Email = e.Email,
                           Password = e.Password,
                           UserRole = "User"
                       };
            return user.FirstOrDefault();
        }

        public string GenerateJWTToken(UserLoginModel userInfo)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiry = DateTime.Now.AddMinutes(30);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.NameId, userInfo.Email),
                //new Claim("username", userInfo.Email.ToString()),
                //new Claim("role",userInfo.UserRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
