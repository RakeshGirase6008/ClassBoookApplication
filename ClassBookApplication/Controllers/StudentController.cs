using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Student;
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
    public class StudentController : Controller
    {
        #region Ctor

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly LogsService _logsService;
        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor

        public StudentController(ClassBookModelFactory classBookModelFactory,
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

        public IActionResult Register()
        {
            RegisterModel model = new RegisterModel();
            model = LoadModel(ref model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(RegisterModel model)
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
                        Student studentData = new Student()
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Address = model.Address,
                            Email = model.Email,
                            Gender = Enum.GetName(typeof(Gender), model.GenderId),
                            DOB = model.DOB,
                            StateId = model.StateId,
                            CityId = model.CityId,
                            ContactNo = model.ContactNo,
                            Pincode = model.PincodeId,
                            BoardId = model.BoardId,
                            MediumId = model.MediumId,
                            StandardId = model.StandardId
                        };
                        (int studentId, string uniqueNo) = _classBookService.SaveStudent(studentData, list);
                        string UserName = studentData.FirstName + studentData.LastName + uniqueNo;
                        var user = _classBookService.SaveUserData(studentId, Module.Student, UserName, studentData.Email);
                        _classBookService.SendVerificationLinkEmail(studentData.Email, user.Password, Module.Student.ToString());
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
                _logsService.InsertLogs("Student", ex, "Student", 0);
                return RedirectToAction("Register");
            }
        }

        public IActionResult GetCities(int stateId)
        {
            return Json(_classBookModelFactory.PrepareCityDropDown(stateId));
        }

        public IActionResult GetPincodes(int cityId)
        {
            return Json(_classBookModelFactory.PreparePincodeDropDown(cityId));
        }

        #region Utilities
        protected RegisterModel LoadModel(ref RegisterModel model)
        {
            model.States = _classBookModelFactory.PrepareStateDropDown();
            model.GenderList = _classBookModelFactory.PrepareGenderDropDown();
            model.BoardList = _classBookModelFactory.PrepareBoardDropDown();
            model.MediumList = _classBookModelFactory.PrepareMediumDropDown();
            model.StandardList = _classBookModelFactory.PrepareStandardDropDown();
            return model;
        }

        #endregion
    }
}
