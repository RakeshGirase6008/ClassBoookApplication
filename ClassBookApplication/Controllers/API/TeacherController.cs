using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Teacher;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using ClassBookApplication.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ClassBookApplication.Controllers.API
{
    [ApiVersion("1")]
    public class TeacherController : MainApiController
    {

        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly LogsService _logsService;
        private readonly ClassBookModelFactory _classBookModelFactory;
        


        #endregion

        #region Ctor

        public TeacherController(ClassBookManagementContext context,
            ClassBookService classBookService,
            LogsService logsService,
            ClassBookModelFactory classBookModelFactory)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._logsService = logsService;
            this._classBookModelFactory = classBookModelFactory;
        }

        #endregion

        #region Register Teacher

        // POST api/Teacher/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Teacher teacherData = JsonConvert.DeserializeObject<Teacher>(model.data.ToString());
                    if (teacherData != null)
                    {
                        var singleUser = _context.Teacher.Where(x => x.Email == teacherData.Email).AsNoTracking();
                        if (!singleUser.Any())
                        {
                            (int teacherId, string uniqueNo) = _classBookService.SaveTeacher(teacherData, model.files);
                            string UserName = teacherData.FirstName + uniqueNo;
                            _classBookService.SaveMappingData((int)Module.Teacher, teacherId, teacherData.MappingRequestModel);
                            var user = _classBookService.SaveUserData(teacherId, Module.Teacher, UserName, teacherData.Email, model.FCMId, model.DeviceId);
                            await Task.Run(() => _classBookService.SendVerificationLinkEmail(teacherData.Email, user.Password, Module.Teacher.ToString()));
                            var succeeModel = new
                            {
                                Message = ClassBookConstantString.Register_Teacher_Success.ToString(),
                                Data = _classBookModelFactory.PrepareUserDetail(user)
                            };
                            return StatusCode((int)HttpStatusCode.OK, succeeModel);
                        }
                        else
                        {
                            var authorizeAccess = new
                            {
                                Message = ClassBookConstantString.Validation_EmailExist.ToString()
                            };
                            return StatusCode((int)HttpStatusCode.Conflict, authorizeAccess);
                        }
                    }
                    return Ok();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
                }
                
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Teacher, exception, "api/Teacher/Register", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }

        // POST api/Teacher/EditTeacher
        [HttpPost("EditTeacher")]
        public IActionResult EditTeacher([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Teacher teacherData = JsonConvert.DeserializeObject<Teacher>(model.data.ToString());
                    if (teacherData != null)
                    {
                        if (_context.Teacher.Count(x => x.Email == teacherData.Email && x.Id != teacherData.Id) > 0)
                        {
                            var authorizeAccess = new
                            {
                                Message = ClassBookConstantString.Validation_EmailExist.ToString()
                            };
                            return StatusCode((int)HttpStatusCode.Conflict, authorizeAccess);
                        }
                        else
                        {
                            var singleTeacher = _context.Teacher.Where(x => x.Id == teacherData.Id).AsNoTracking().FirstOrDefault();
                            int teacherId = _classBookService.UpdateTeachers(teacherData, singleTeacher, model.files);
                            _classBookService.SaveMappingData((int)Module.Teacher, teacherId, teacherData.MappingRequestModel);
                            var exceptionModel = new
                            {
                                Message = ClassBookConstantString.Edit_Teacher_Success.ToString(),
                            };
                            return StatusCode((int)HttpStatusCode.OK, exceptionModel);
                        }
                    }
                    return Ok();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
                }
                
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Teacher, exception, "api/Teacher/EditTeacher", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }

        #endregion

        #region GetTeacherDetails

        // GET api/Teacher/GetAllTeacher
        [HttpGet("GetAllTeacher")]
        public IEnumerable<ListingModel> GetAllTeacher()
        {
            var teachersData = from teacher in _context.Teacher
                               where teacher.Active == true
                               select new ListingModel
                               {
                                   Id = teacher.Id,
                                   Title = teacher.FirstName + " " + teacher.LastName,
                                   Image = "https://classbookapplication.appspot.com/" + teacher.ProfilePictureUrl.Replace("\\", "/"),
                                   Rating = 5,
                                   TotalBoard = 2,
                                   TotalStandard = 3,
                                   TotalSubject = 4
                               };
            return teachersData;
        }

        // GET api/Teacher/GetTeacherById/5
        [HttpGet("GetTeacherById/{id:int}")]
        public IEnumerable<Teacher> GetTeacherById(int id)
        {
            var teacher = _context.Teacher.Where(x => x.Id == id).AsEnumerable();
            return teacher;
        }
        #endregion
    }
}
