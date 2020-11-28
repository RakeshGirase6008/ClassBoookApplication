using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.CareerExpert;
using ClassBookApplication.Domain.School;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.PublicModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassBookApplication.Controllers
{
    public class CareerExpertController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly LogsService _logsService;
        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor

        public CareerExpertController(ClassBookModelFactory classBookModelFactory,
            LogsService logsService,
            ClassBookManagementContext context,
            ClassBookService classBookService)
        {
            _classBookModelFactory = classBookModelFactory;
            _logsService = logsService;
            _context = context;
            _classBookService = classBookService;
        }

        #endregion

        #region Method

        public IActionResult Register()
        {
            CareerExpertRegisterModel model = new CareerExpertRegisterModel();
            model = LoadModel(ref model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(CareerExpertRegisterModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var singleUser = _context.Users.Where(x => x.Email == model.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        List<IFormFile> list = new List<IFormFile>();
                        if (model.ImageFile != null)
                            list.Add(model.ImageFile);

                        CareerExpert careerExpertData = new CareerExpert()
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            AlternateContact = model.AlternateContact,
                            ContactNo = model.ContactNo,
                            DOB = model.DOB,
                            Description = model.Description,
                            Address = model.Address,
                            Gender = Enum.GetName(typeof(Gender), model.GenderId),
                            StateId = model.StateId,
                            CityId = model.CityId,
                            Pincode = model.PincodeId,
                            TeachingExperience = model.TeachingExperience
                        };
                        (int studentId, string uniqueNo) = _classBookService.SaveCareerExpert(careerExpertData, list);
                        string UserName = careerExpertData.FirstName + uniqueNo;
                        var user = _classBookService.SaveUserData(studentId, Module.Student, UserName, careerExpertData.Email);
                        //var rest = _classBookService.RegisterMethod(model, "/api/v1/ChannelPartner/register");
                        _classBookService.SendVerificationLinkEmail(careerExpertData.Email, user.Password, Module.Student.ToString());
                        return RedirectToAction("Register");
                    }
                    else
                    {
                        ModelState.AddModelError("Email", "Email Id Already Exist");
                        model = LoadModel(ref model);
                    }
                    return View(model);
                }
                else
                {
                    model = LoadModel(ref model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logsService.InsertLogs("CareerExpert", ex, "CareerExpert", 0);
                return RedirectToAction("Register");
            }
        }

        #endregion

        #region Utilities

        protected CareerExpertRegisterModel LoadModel(ref CareerExpertRegisterModel model)
        {
            model.States = _classBookModelFactory.PrepareStateDropDown();
            model.GenderList = _classBookModelFactory.PrepareGenderDropDown(); 
            return model;
        }

        #endregion
    }
}
