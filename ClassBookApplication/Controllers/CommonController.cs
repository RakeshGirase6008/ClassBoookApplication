using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.PublicModel;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ClassBookApplication.Controllers
{
    public class CommonController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly ClassBookManagementContext _context;

        #endregion

        #region Ctor

        public CommonController(ClassBookModelFactory classBookModelFactory,
            ClassBookManagementContext context)
        {
            _classBookModelFactory = classBookModelFactory;
            _context = context;
        }

        #endregion

        #region Methods
        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }
        #endregion

        #region Utilities
        public IActionResult GetCities(int stateId)
        {
            return Json(_classBookModelFactory.PrepareCityDropDown(stateId));
        }

        public IActionResult GetPincodes(int cityId)
        {
            return Json(_classBookModelFactory.PreparePincodeDropDown(cityId));
        }

        [HttpPost]
        public IActionResult SendContactUs(ContactUsModel model)
        {
            try
            {
                ContactUs entity = new ContactUs();
                entity.Name = model.Name;
                entity.EmailId = model.EmailId;
                entity.Message = model.Message;
                entity.MobileNo = model.MobileNo;
                entity.FromType = "Website";
                entity.Active = true;
                _context.ContactUs.Add(entity);
                _context.SaveChanges();
                return Json(new { status = "true", message = "We will contact you as soon as possible" });
            }
            catch(Exception ex)
            {
                return Json(new { status = "true", message = "Getting issue" });
            }
        }

        #endregion
    }
}
