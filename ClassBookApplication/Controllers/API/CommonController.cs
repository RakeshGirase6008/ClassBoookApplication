using ClassBookApplication.DataContext;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ClassBookApplication.Controllers.API
{
    [ApiVersion("1")]
    [ApiVersion("2")]
    public class CommonController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly LogsService _logsService;
        private readonly ClassBookService _classBookService;
        private readonly ClassBookModelFactory _classBookModelFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;


        #endregion

        #region Ctor

        public CommonController(ClassBookManagementContext context,
            LogsService logsService,
            ClassBookService classBookService,
            ClassBookModelFactory classBookModelFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            this._context = context;
            this._logsService = logsService;
            this._classBookService = classBookService;
            this._classBookModelFactory = classBookModelFactory;
            this._httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region Common

        // GET api/Common/GetBoard
        [HttpGet("GetBoard")]
        public IEnumerable<object> GetBoard()
        {
            var boards = _context.Board.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return boards;
        }

        // GET api/Common/GetStates
        [HttpGet("GetStates")]
        public IEnumerable<object> GetStates()
        {
            var States = _context.States.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return States;
        }

        // GET api/Common/GetCities
        [HttpGet("GetCities")]
        public IEnumerable<object> GetCities()
        {
            var cities = _context.City.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return cities;
        }

        // GET api/Common/GetCitiesByStateId/6
        [HttpGet("GetCitiesByStateId/{id:int}")]
        public IEnumerable<object> GetCities(int id)
        {
            var cityData = from city in _context.City
                           where city.StateId == id && city.Active == true
                           select new { city.Name, city.Id };
            return cityData;
        }

        // GET api/Common/GetPincodes
        [HttpGet("GetPincodes")]
        public IEnumerable<object> GetPincodes()
        {
            var pincodes = _context.Pincode.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return pincodes;
        }

        // GET api/Common/GetPincodeByCityId/6
        [HttpGet("GetPincodeByCityId/{id:int}")]
        public IEnumerable<object> GetPincodeByCityId(int id)
        {
            var cityData = from pincode in _context.Pincode
                           where pincode.CityId == id && pincode.Active == true
                           select new { pincode.Name, pincode.Id };
            return cityData;
        }


        // GET api/Common/GetStandard
        [HttpGet("GetStandard")]
        public IEnumerable<object> GetStandard()
        {
            var standard = _context.Standards.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return standard;
        }

        // GET api/Common/GetMedium
        [HttpGet("GetMedium")]
        public IEnumerable<object> GetMedium()
        {
            var medium = _context.Medium.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return medium;
        }

        // GET api/Common/GetSubject
        [HttpGet("GetSubject")]
        public IEnumerable<object> GetSubject()
        {
            var subject = _context.Subjects.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return subject;
        }

        // GET api/Common/GetSubjectSpeciality
        [HttpGet("GetSubjectSpeciality")]
        public IEnumerable<object> GetSubjectSpeciality()
        {
            var subjectSpeciality = _context.SubjectSpeciality.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return subjectSpeciality;
        }

        // GET api/Common/GetCourseCategory
        [HttpGet("GetCourseCategory")]
        public IEnumerable<object> GetCourseCategory()
        {
            var courseCategory = _context.CourseCategory.Where(x => x.Active == true).Select(x => new { x.Name, x.Id });
            return courseCategory;
        }

        #endregion

        #region User API

        // POST api/Common/Login
        [HttpPost("Login")]
        public IActionResult Login([FromForm] LoginModel model)
        {
            ResponseModel responseModel = new ResponseModel();

            if (model != null)
            {
                var singleUser = _context.Users.Where(x => x.Email == model.Email && x.Password == model.Password).AsNoTracking();
                if (singleUser.Any())
                {
                    // Update UserData
                    var user = singleUser.FirstOrDefault();
                    user.AuthorizeTokenKey = _classBookService.GenerateAuthorizeTokenKey();
                    user.FCMId = model.FCMId;
                    _context.Users.Update(user);
                    _context.SaveChanges();

                    // Save AuthorizationDevice Data
                    _classBookService.SaveDeviceAuthorizationData(user, model.DeviceId);
                    responseModel.Message = ClassBookConstantString.Login_Success.ToString();
                    responseModel.Data = _classBookModelFactory.PrepareUserDetail(user);
                    return StatusCode((int)HttpStatusCode.OK, responseModel);
                }
                else
                {
                    responseModel.Message = "Email & Password not matching for specified data";
                    return StatusCode((int)HttpStatusCode.Unauthorized, responseModel);

                }
            }
            return Ok();

        }


        // POST api/Common/ForgotPassword
        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword([FromForm] ForgotPassword model)
        {
            ResponseModel responseModel = new ResponseModel();
            var singleUser = _context.Users.Where(x => x.Email == model.Email).AsNoTracking();
            if (singleUser.Any())
            {
                var user = singleUser.FirstOrDefault();
                _classBookService.SendVerificationLinkEmail(user.Email, user.Password, "Forgot Password");
                responseModel.Message = "Please check your email Id for password";
                return StatusCode((int)HttpStatusCode.OK, responseModel);
            }
            else
            {
                responseModel.Message = "Email Id is not exist";
                return StatusCode((int)HttpStatusCode.NotFound, responseModel);
            }

        }

        // POST api/Common/ChangePassword
        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromForm] ChangePassword model)
        {
            ResponseModel responseModel = new ResponseModel();
            string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
            var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey && x.Password == model.OldPassword).AsNoTracking();
            if (singleUser.Any())
            {
                var user = singleUser.FirstOrDefault();
                user.Password = model.NewPassword;
                _context.Users.Update(user);
                _context.SaveChanges();
                responseModel.Message = "Password change Successfully";
                return StatusCode((int)HttpStatusCode.OK, responseModel);
            }
            else
            {
                responseModel.Message = "Old Password is not matching";
                return StatusCode((int)HttpStatusCode.NotFound, responseModel);
            }
        }

        #endregion

        #region GetAllCommonData

        // POST api/Common/GetSubjectsByModuleId
        [HttpPost("GetSubjectsByModuleId")]
        public object GetSubjectsByModuleId([FromForm] SubjectRequestDetails subjectRequestDetails)
        {
            return _classBookService.GetSubjects(subjectRequestDetails);
        }
        #endregion

        //#region Version Sample Only

        //// GET api/Common/GetBoardSample
        //[HttpGet("GetBoardSample")]
        //public IEnumerable<string> GetBoard_V1()
        //{
        //    return new string[] { "version 1.1 value 1", "version 1.1 value2 " };
        //}

        //// GET api/Common/GetBoardSample
        //[HttpGet("GetBoardSample")]
        //[MapToApiVersion("2.0")]
        //public IEnumerable<string> GetBoard_V2()
        //{
        //    return new string[] { "version 1.1 value 1", "version 1.1 value2 " };
        //}

        //#endregion
    }
}