using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Classes;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.PublicModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using JW;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassBookApplication.Controllers
{
    public class ClassController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly LogsService _logsService;
        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;

        #endregion

        #region Ctor

        public ClassController(ClassBookModelFactory classBookModelFactory,
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
            ClassRegisterModel model = new ClassRegisterModel();
            model = LoadModel(ref model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Register(ClassRegisterModel model)
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

                        Classes classData = new Classes()
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
                        (int studentId, string uniqueNo) = _classBookService.SaveClasses(classData, list);
                        string UserName = classData.Name + uniqueNo;
                        var user = _classBookService.SaveUserData(studentId, Module.Student, UserName, classData.Email);
                        //var rest = _classBookService.RegisterMethod(model, "/api/v1/ChannelPartner/register");
                        _classBookService.SendVerificationLinkEmail(classData.Email, user.Password, Module.Student.ToString());
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
                _logsService.InsertLogs("Classes", ex, "Classes", 0);
                return RedirectToAction("Register");
            }
        }

        [HttpGet]
        public IActionResult AllClassesList(int p)
        {
            ClassListModel model = new ClassListModel();
            if (p == 0)
                p = 1;
            int count = 0;
            model.ClassModel = _classBookService.AllClassesList(out count, p, 3);
            model.Pager = new Pager(count, p, 3);
            return View(model);

            //return View();

        }

        #endregion

        #region Utilities

        protected ClassRegisterModel LoadModel(ref ClassRegisterModel model)
        {
            model.States = _classBookModelFactory.PrepareStateDropDown();
            return model;
        }

        #endregion
    }
}
