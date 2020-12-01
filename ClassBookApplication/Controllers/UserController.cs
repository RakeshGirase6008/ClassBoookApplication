using ClassBookApplication.DataContext;
using ClassBookApplication.Models.PublicModel;
using ClassBookApplication.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ClassBookApplication.Controllers
{
    public class UserController : Controller
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly MyAuthencationService _myAuthencationService;
        #endregion

        #region Ctor

        public UserController(ClassBookManagementContext context,
            ClassBookService classBookService,
            MyAuthencationService myAuthencationService)
        {
            _context = context;
            _classBookService = classBookService;
            _myAuthencationService = myAuthencationService;
        }

        #endregion

        #region Action
        public IActionResult SendForgotPasword(string emailId)
        {
            var singleUser = _context.Users.Where(x => x.Email == emailId).AsNoTracking();
            if (singleUser.Any())
            {
                var user = singleUser.FirstOrDefault();
                _classBookService.SendVerificationLinkEmail(user.Email, user.Password, "Forgot Password");
                return Json(new { status = "true", message = "Email Sent Successfully" });
            }
            else
            {
                return Json(new { status = "false", message = "Email not Exist" });
            }
        }


        [HttpGet]
        [Authorize]
        public IActionResult ForgotPassword()
        {

            //context.HttpContext.Request?.Headers["Basic"];
            var xyz = HttpContext.Request?.Headers["Basic"];
            var principal = HttpContext.User;
            if (principal?.Claims != null)
            {
                foreach (var claim in principal.Claims)
                {
                }
            }
            string email = principal?.Claims?.SingleOrDefault(p => p.Type == "email")?.Value;

            //string token = new JwtSecurityTokenHandler().ReadJwtToken("");
            //JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            //tokenHandler.rea
            //TokenValidationParameters validationParameters;
            //SecurityToken securityToken;
            //IPrincipal principal;
            //try
            //{
            //    // token validation
            //    principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
            //    // Reading the "verificationKey" claim value:
            //    var vk = principal.Claims.SingleOrDefault(c => c.Type == "verificationKey").Value;
            //}
            //catch
            //{
            //    principal = null; // token validation error
            //}
            //// Cast to ClaimsIdentity.
            //var identity = HttpContext.User.Identity as ClaimsIdentity;

            //// Gets list of claims.
            //IEnumerable<Claim> claim = identity.Claims;

            //// Gets name from claims. Generally it's an email address.
            //var usernameClaim = claim
            //    .Where(x => x.Type == ClaimTypes.Name)
            //    .FirstOrDefault();
            //var user = usernameClaim.Value;
            ForgotPasswordModel model = new ForgotPasswordModel();
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            UserLoginModel model = new UserLoginModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login(UserLoginModel model)
        {
            UserLoginModel user = _myAuthencationService.AuthenticateUser(model);
            if (user != null)
            {
                var tokenString = _myAuthencationService.GenerateJWTToken(user);
                HttpContext.Session.SetString("JWToken", tokenString);
                //return RedirectToAction("List");
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public IActionResult Logoff()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }


        #endregion
    }
}
