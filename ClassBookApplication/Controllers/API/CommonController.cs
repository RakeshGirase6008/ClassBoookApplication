using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

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

        #endregion

        #region Ctor

        public CommonController(ClassBookManagementContext context,
            LogsService logsService,
            ClassBookService classBookService)
        {
            this._context = context;
            this._logsService = logsService;
            this._classBookService = classBookService;
        }
        #endregion

        #region Common

        // GET api/Common/GetBoard
        [HttpGet("GetBoard")]
        public IEnumerable<Board> GetBoard()
        {
            var boards = _context.Board.Where(x => x.Active == true).AsEnumerable();
            return boards;
        }

        // GET api/Common/GetStates
        [HttpGet("GetStates")]
        public IEnumerable<States> GetStates()
        {
            var States = _context.States.Where(x => x.Active == true).AsEnumerable();
            return States;
        }

        // GET api/Common/GetCities
        [HttpGet("GetCities")]
        public IEnumerable<City> GetCities()
        {
            var cities = _context.City.Where(x => x.Active == true).AsEnumerable();
            return cities;
        }

        // GET api/Common/GetCitiesByStateId/6
        [HttpGet("GetCitiesByStateId/{id:int}")]
        public IEnumerable<object> GetCities(int id)
        {
            var cityData = from city in _context.City
                           where city.StateId == id && city.Active == true
                           select new { city.Name };
            return cityData;
        }

        // GET api/Common/GetStandard
        [HttpGet("GetStandard")]
        public IEnumerable<Standards> GetStandard()
        {
            var standard = _context.Standards.Where(x => x.Active == true).AsEnumerable();
            return standard;
        }

        // GET api/Common/GetMedium
        [HttpGet("GetMedium")]
        public IEnumerable<Medium> GetMedium()
        {
            var medium = _context.Medium.Where(x => x.Active == true).AsEnumerable();
            return medium;
        }

        // GET api/Common/GetSubject
        [HttpGet("GetSubject")]
        public IEnumerable<Subjects> GetSubject()
        {
            var subject = _context.Subjects.Where(x => x.Active == true).AsEnumerable();
            return subject;
        }

        // GET api/Common/GetSubjectSpeciality
        [HttpGet("GetSubjectSpeciality")]
        public IEnumerable<SubjectSpeciality> GetSubjectSpeciality()
        {
            var subjectSpeciality = _context.SubjectSpeciality.Where(x => x.Active == true).AsEnumerable();
            return subjectSpeciality;
        }

        // GET api/Common/GetCourseCategory
        [HttpGet("GetCourseCategory")]
        public IEnumerable<CourseCategory> GetCourseCategory()
        {
            var courseCategory = _context.CourseCategory.Where(x => x.Active == true).AsEnumerable();
            return courseCategory;
        }

        #endregion

        #region User API

        // POST api/Common/Login
        [HttpPost("Login")]
        public IActionResult Login(LoginModel model)
        {
            CommonResponseModel exceptionModel = new CommonResponseModel();
            try
            {
                if (model != null)
                {
                    var singleUser = _context.Users.Where(x => x.Email == model.Email && x.Password == model.Password).AsNoTracking();
                    if (singleUser.Any())
                    {
                        exceptionModel.Status = true;
                        exceptionModel.Message = ClassBookConstantString.Login_Success.ToString();
                    }
                    else
                    {
                        exceptionModel.Status = false;
                        exceptionModel.Message = ClassBookConstantString.Login_Failed.ToString();
                    }
                }
                return Ok(exceptionModel);
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Login, exception, "api/Common/Login", 0);
                exceptionModel.Status = false;
                exceptionModel.Message = ClassBookConstantString.Login_Failed.ToString();
                exceptionModel.ErrorMessage.Add(exception?.Message);
                exceptionModel.ErrorMessage.Add(exception?.InnerException?.ToString());
                return Ok(exceptionModel);
            }
        }


        // POST api/Common/ForgotPassword
        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword(LoginModel model)
        {
            CommonResponseModel exceptionModel = new CommonResponseModel();
            try
            {
                if (model != null)
                {
                    var singleUser = _context.Users.Where(x => x.Email == model.Email).AsNoTracking();
                    if (singleUser.Any())
                    {
                        var user = singleUser.FirstOrDefault();
                        _classBookService.SendVerificationLinkEmail(model.Email, user.Password, "Forgot Password");
                        exceptionModel.Status = true;
                        exceptionModel.Message = "Please check your email Id for password";
                    }
                    else
                    {
                        exceptionModel.Status = false;
                    }
                }
                return Ok(exceptionModel);
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Login, exception, "api/Common/ForgotPassword", 0);
                exceptionModel.Status = false;
                exceptionModel.ErrorMessage.Add(exception?.Message);
                exceptionModel.ErrorMessage.Add(exception?.InnerException?.ToString());
                return Ok(exceptionModel);
            }
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