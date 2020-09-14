using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Classes;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ClassBookApplication.Controllers.API
{
    [ApiVersion("1")]
    public class ClassesController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly ClassBookModelFactory _classBookModelFactory;

        #endregion

        #region Ctor

        public ClassesController(ClassBookManagementContext context,
            ClassBookService classBookService,
            ClassBookModelFactory classBookModelFactory)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._classBookModelFactory = classBookModelFactory;
        }

        #endregion

        #region Register Classes

        // POST api/Classes/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                Classes classesData = JsonConvert.DeserializeObject<Classes>(model.data.ToString());
                if (classesData != null)
                {
                    var singleUser = _context.Users.Where(x => x.Email == classesData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int classesId, string uniqueNo) = _classBookService.SaveClasses(classesData, model.files);
                        string UserName = classesData.Name + uniqueNo;
                        //_classBookService.SaveMappingData((int)Module.Classes, classesId, classesData.MappingRequestModel);
                        var user = _classBookService.SaveUserData(classesId, Module.Classes, UserName, classesData.Email, model.FCMId, model.DeviceId);
                        await Task.Run(() => _classBookService.SendVerificationLinkEmail(classesData.Email, user.Password, Module.Classes.ToString()));
                        responseModel.Message = ClassBookConstantString.Login_Success.ToString();
                        responseModel.Data = _classBookModelFactory.PrepareUserDetail(user);
                        return StatusCode((int)HttpStatusCode.OK, responseModel);
                    }
                    else
                    {
                        responseModel.Message = ClassBookConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                }
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }

        }

        // POST api/Classes/EditClasses
        [HttpPost("EditClasses")]
        public IActionResult EditClasses([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                Classes classesData = JsonConvert.DeserializeObject<Classes>(model.data.ToString());
                if (classesData != null)
                {
                    if (_context.Users.Count(x => x.Email == classesData.Email && x.UserId != classesData.Id) > 0)
                    {
                        responseModel.Message = ClassBookConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                    else
                    {
                        var singleClass = _context.Classes.Where(x => x.Id == classesData.Id).AsNoTracking().FirstOrDefault();
                        int classId = _classBookService.UpdateClasses(classesData, singleClass, model.files);
                        //_classBookService.SaveMappingData((int)Module.Classes, classId, classesData.MappingRequestModel);
                        responseModel.Message = ClassBookConstantString.Edit_Classes_Success.ToString();
                        return StatusCode((int)HttpStatusCode.OK, responseModel);
                    }
                }
                return StatusCode((int)HttpStatusCode.BadRequest);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }
        }

        #endregion

        #region GetClassesDetails

        // GET api/Classes/GetAllClasses
        [HttpGet("GetAllClasses")]
        public IEnumerable<ListingModel> GetAllClasses()
        {
            return _classBookService.GetModuleDataByModuleId((int)Module.Classes);
        }

        // GET api/Classes/GetClassById/5
        [HttpGet("GetClassById/{id:int}")]
        public object GetClassById(int id)
        {
            var query = from classes in _context.Classes
                        join city in _context.City on classes.CityId equals city.Id
                        where classes.Id == id && classes.Active == true
                        orderby classes.Id
                        select new ClassDetailModel
                        {
                            Id = classes.Id,
                            Name = classes.Name,
                            CityName = city.Name,
                            IntroductionURL = classes.IntroductionURL
                        };
            var ClassData = query.FirstOrDefault();
            ClassData.ClassDetailModels = _classBookService.GetClassDetailByClassId(ClassData.Id);
            return ClassData;
        }

        // GET api/Classes/GetClassById/5
        [HttpPost("GetSubjects")]
        public object GetSubjects([FromForm] SubjectRequestDetails subjectRequestDetails)
        {
            return _classBookService.GetSubjects(subjectRequestDetails, (int)Module.Classes);
        }

        #endregion
    }
}
