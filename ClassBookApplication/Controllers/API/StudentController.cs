using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Student;
using ClassBookApplication.Factory;
using ClassBookApplication.Models.RequestModels;
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

    public class StudentController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly LogsService _logsService;
        private readonly ClassBookModelFactory _classBookModelFactory;


        #endregion

        #region Ctor

        public StudentController(ClassBookManagementContext context,
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

        #region Register User

        // POST api/Student/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Student studentData = JsonConvert.DeserializeObject<Student>(model.data.ToString());
                    if (studentData != null)
                    {
                        var singleUser = _context.Student.Where(x => x.Email == studentData.Email).AsNoTracking();
                        if (!singleUser.Any())
                        {
                            (int studentId, string uniqueNo) = _classBookService.SaveStudent(studentData, model.files);
                            string UserName = studentData.FirstName + studentData.LastName + uniqueNo;
                            var user = _classBookService.SaveUserData(studentId, Module.Student, UserName, studentData.Email, model.FCMId, model.DeviceId);
                            await Task.Run(() => _classBookService.SendVerificationLinkEmail(studentData.Email, user.Password, Module.Student.ToString()));
                            var succeeModel = new
                            {
                                Message = ClassBookConstantString.Register_Student_Success.ToString(),
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
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Student, exception, "api/student/register", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }

        // POST api/Student/EditStudent
        [HttpPost("EditStudent")]
        public IActionResult EditStudent([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Student studentData = JsonConvert.DeserializeObject<Student>(model.data.ToString());
                    if (studentData != null)
                    {
                        if (_context.Student.Count(x => x.Email == studentData.Email && x.Id != studentData.Id) > 0)
                        {
                            var authorizeAccess = new
                            {
                                Message = ClassBookConstantString.Validation_EmailExist.ToString()
                            };
                            return StatusCode((int)HttpStatusCode.Conflict, authorizeAccess);
                        }
                        else
                        {
                            var singleUser = _context.Student.Where(x => x.Id == 1).AsNoTracking().FirstOrDefault();
                            int studentId = _classBookService.UpdateStudent(studentData, singleUser, model.files);
                            var exceptionModel = new
                            {
                                Message = ClassBookConstantString.Edit_Student_Success.ToString(),
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
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Student, exception, "api/student/EditStudent", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }
        #endregion

        #region GetStudentDetails

        // GET api/Student/GetAllStudents
        [HttpGet("GetAllStudents")]
        public IEnumerable<Student> GetAllStudents()
        {
            var students = _context.Student.Where(x => x.Active == true && x.Deleted == true).AsEnumerable();
            return students;
        }

        // GET api/Student/GetStudentById/5
        [HttpGet("GetStudentById/{id:int}")]
        public IEnumerable<Student> GetStudentById(int id)
        {
            var students = _context.Student.Where(x => x.Id == id).AsEnumerable();
            return students;
        }
        #endregion
    }
}