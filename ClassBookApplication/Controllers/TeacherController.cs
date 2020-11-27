using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Teacher;
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
    public class TeacherController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly LogsService _logsService;
        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor

        public TeacherController(ClassBookModelFactory classBookModelFactory,
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
            TeacherRegisterModel model = new TeacherRegisterModel();
            model = LoadModel(ref model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(TeacherRegisterModel model)
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

                        Teacher teacherData = new Teacher()
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
                        (int teacherId, string uniqueNo) = _classBookService.SaveTeacher(teacherData, list);
                        string UserName = teacherData.FirstName + uniqueNo;
                        var user = _classBookService.SaveUserData(teacherId, Module.Student, UserName, teacherData.Email);
                        //var rest = _classBookService.RegisterMethod(model, "/api/v1/ChannelPartner/register");
                        _classBookService.SendVerificationLinkEmail(teacherData.Email, user.Password, Module.Student.ToString());
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
                _logsService.InsertLogs("Teacher", ex, "Teacher", 0);
                return RedirectToAction("Register");
            }
        }

        #endregion

        #region Utilities

        protected TeacherRegisterModel LoadModel(ref TeacherRegisterModel model)
        {
            model.States = _classBookModelFactory.PrepareStateDropDown();
            model.GenderList = _classBookModelFactory.PrepareGenderDropDown();
            return model;
        }

        #endregion
    }
}
