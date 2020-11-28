using ClassBookApplication.DataContext;
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
    public class SchoolController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly LogsService _logsService;
        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor

        public SchoolController(ClassBookModelFactory classBookModelFactory,
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
            SchoolRegisterModel model = new SchoolRegisterModel();
            model = LoadModel(ref model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(SchoolRegisterModel model)
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

                        School schoolData = new School()
                        {
                            Name = model.Name,
                            Email = model.Email,
                            AlternateContact = model.AlternateContact,
                            ContactNo = model.ContactNo,
                            EstablishmentDate = model.EstablishmentDate,
                            Address = model.Address,
                            StateId = model.StateId,
                            CityId = model.CityId,
                            Pincode = model.PincodeId,
                            RegistrationNo = model.RegistrationNo,
                            TeachingExperience = model.TeachingExperience
                        };
                        (int studentId, string uniqueNo) = _classBookService.SaveSchool(schoolData, list);
                        string UserName = schoolData.Name + uniqueNo;
                        var user = _classBookService.SaveUserData(studentId, Module.Student, UserName, schoolData.Email);
                        //var rest = _classBookService.RegisterMethod(model, "/api/v1/ChannelPartner/register");
                        _classBookService.SendVerificationLinkEmail(schoolData.Email, user.Password, Module.Student.ToString());
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
                _logsService.InsertLogs("School", ex, "School", 0);
                return RedirectToAction("Register");
            }
        }

        #endregion

        #region Utilities

        protected SchoolRegisterModel LoadModel(ref SchoolRegisterModel model)
        {
            model.States = _classBookModelFactory.PrepareStateDropDown();
            return model;
        }

        #endregion
    }
}
