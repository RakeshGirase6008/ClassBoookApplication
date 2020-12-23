using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.PublicModel;
using ClassBookApplication.Service;
using JW;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ClassBookApplication.Controllers
{
    public class CommonController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;


        #endregion

        #region Ctor

        public CommonController(ClassBookModelFactory classBookModelFactory,
            ClassBookManagementContext context,
            ClassBookService classBookService)
        {
            _classBookModelFactory = classBookModelFactory;
            _context = context;
            _classBookService = classBookService;
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

        [HttpGet]
        [Route("Subject/{smbId}")]
        public IActionResult GetSubjectDetails(int smbId)
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
            catch (Exception ex)
            {
                return Json(new { status = "true", message = "Getting issue" });
            }
        }
        public IActionResult Testimonial(int p)
        {
            TestimonialModel model = new TestimonialModel();
            if (p == 0)
                p = 1;
            var dummyTestimonial = _classBookService.GetAllTestimonials();
            model.Pager = new Pager(dummyTestimonial.Count, p, 2, 2);
            model.Testimonials = _classBookModelFactory.PrepareTestimonial(_classBookService.GetAllTestimonials(false, p, 2));
            return View(model);
        }
        #endregion
    }
}
