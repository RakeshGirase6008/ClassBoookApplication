using ClassBookApplication.DataContext;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.PublicModel;
using ClassBookApplication.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ClassBookApplication.Controllers
{
    public class UserController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor

        public UserController(ClassBookModelFactory classBookModelFactory,
            ClassBookManagementContext context,
            ClassBookService classBookService)
        {
            _classBookModelFactory = classBookModelFactory;
            _context = context;
            _classBookService = classBookService;
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
                return Json(new { status = "true" , message ="Email Sent Successfully" });
            }
            else
            {
                return Json(new { status = "false", message = "Email not Exist" });
            }

        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ForgotPasswordModel model = new ForgotPasswordModel();
            return View(model);
        }

        #endregion
    }
}
