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
            ForgotPasswordModel model = new ForgotPasswordModel();
            var userId = _myAuthencationService.GetUserId();
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
