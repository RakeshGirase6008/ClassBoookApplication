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
using System.Threading.Tasks;

namespace ClassBookApplication.Controllers
{
    public class ClassController : Controller
    {
        #region Fields

        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly LogsService _logsService;
        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly IViewRenderService _viewRenderService;

        #endregion

        #region Ctor

        public ClassController(ClassBookModelFactory classBookModelFactory,
            LogsService logsService,
            ClassBookManagementContext context,
            ClassBookService classBookService,
            IViewRenderService viewRenderService)
        {
            _classBookModelFactory = classBookModelFactory;
            _logsService = logsService;
            _context = context;
            _classBookService = classBookService;
            _viewRenderService = viewRenderService;
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
        public IActionResult AllClassesList()
        {
            int count = 0;
            ClassListModel model = new ClassListModel();
            model.States = _classBookModelFactory.PrepareStateDropDown();
            model.BoardList = _classBookModelFactory.PrepareBoardDropDown();
            model.MediumList = _classBookModelFactory.PrepareMediumDropDown();
            model.StandardList = _classBookModelFactory.PrepareStandardDropDown();
            FilterParameter filterParameterModel = new FilterParameter();
            model.ClassModel = _classBookService.AllClassesListModel(filterParameterModel, out count, 1);
            model.Pager = new Pager(count, 1);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AllClassesList(FilterParameter model)
        {
            int count;
            ClassListModel classmodel1 = new ClassListModel();
            classmodel1.ClassModel = _classBookService.AllClassesListModel(model, out count, model.PageIndex);
            classmodel1.Pager = new Pager(count, model.PageIndex);
            var result = await _viewRenderService.RenderToStringAsync("_ClassListPartialView", classmodel1);
            return Content(result);
        }

        [HttpGet]
        public IActionResult GetClass(int id)
        {
            var query = from classes in _context.Classes
                        where classes.Id == id && classes.Active == true
                        orderby classes.Id
                        select new ClassDetailModel
                        {
                            Id = classes.Id,
                            ClassName = classes.Name,
                            Address = classes.Address,
                            Email = classes.Email,
                            ContactNo = classes.ContactNo,
                            Website = "http://abcd.com/",
                            IntroductionVideoUrl = _classBookModelFactory.PrepareURL(classes.IntroductionURL),
                            ProfilePhoto = _classBookModelFactory.PrepareURL(classes.ClassPhotoUrl),
                            Description = classes.Description,
                            TotalRating = 4,
                        };
            ClassDetailModel classData = query.FirstOrDefault();
            var query1 = from smb in _context.StandardMediumBoardMapping
                         join board in _context.Board on smb.BoardId equals board.Id
                         join medium in _context.Medium on smb.MediumId equals medium.Id
                         join std in _context.Standards on smb.StandardId equals std.Id
                         where smb.EntityId == id && smb.ModuleId == (int)Module.Classes
                         select new StandardMediumBoardMappingData
                         {
                             SmbId= smb.Id,
                             BoardId = board.Id,
                             BoardName = board.Name,
                             MediumId = medium.Id,
                             MediumName = medium.Name,
                             StandardId = std.Id,
                             StandardName = std.Name,
                         };
            var standardMapping = query1.ToList();
            classData.StandardMediumBoardMapping = standardMapping;
            Dictionary<int, int> rating = new Dictionary<int, int>();
            rating.Add(1, 1000);
            rating.Add(2, 2000);
            rating.Add(3, 3000);
            rating.Add(4, 4000);
            rating.Add(5, 5000);
            classData.Ratings = rating.OrderByDescending(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
            return View(classData);
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
