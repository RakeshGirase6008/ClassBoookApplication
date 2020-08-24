using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Student;
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


        #endregion

        #region Ctor

        public StudentController(ClassBookManagementContext context,
            ClassBookService classBookService,
            LogsService logsService)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._logsService = logsService;
        }

        #endregion

        #region Register User

        // POST api/Student/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            CommonResponseModel exceptionModel = new CommonResponseModel();
            try
            {
                Student studentData = JsonConvert.DeserializeObject<Student>(model.data.ToString());
                if (studentData != null)
                {
                    var singleUser = _context.Student.Where(x => x.Email == studentData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int studentId, string uniqueNo) = _classBookService.SaveStudent(studentData, model.files);
                        string UserName = studentData.FirstName + studentData.LastName + uniqueNo;
                        var user = _classBookService.SaveUserData(studentId, Module.Student, UserName, studentData.Email);
                        await Task.Run(() => _classBookService.SendVerificationLinkEmail(studentData.Email, user.Password, Module.Student.ToString()));
                        exceptionModel.Status = true;
                        exceptionModel.Data = user;
                        exceptionModel.Message = ClassBookConstantString.Register_Student_Success.ToString();
                    }
                    else
                    {
                        exceptionModel.Status = false;
                        exceptionModel.Message = ClassBookConstantString.Register_Student_Failed.ToString();
                        exceptionModel.ValidationMessage.Add(ClassBookConstantString.Validation_EmailExist.ToString());
                    }
                }
                return Ok(exceptionModel);
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Student, exception, "api/student/register", 0);
                exceptionModel.Status = false;
                exceptionModel.Message = ClassBookConstantString.Register_Student_Failed.ToString();
                exceptionModel.ErrorMessage.Add(exception?.Message);
                exceptionModel.ErrorMessage.Add(exception?.InnerException?.Message);
                return Ok(exceptionModel);
            }
        }

        // POST api/Student/EditStudent
        [HttpPost("EditStudent")]
        public IActionResult EditStudent([FromForm] CommonRegistrationModel model)
        {
            CommonResponseModel exceptionModel = new CommonResponseModel();
            try
            {
                Student studentData = JsonConvert.DeserializeObject<Student>(model.data.ToString());
                if (studentData != null)
                {
                    if (_context.Student.Count(x => x.Email == studentData.Email && x.Id != studentData.Id) > 0)
                    {
                        exceptionModel.Status = false;
                        exceptionModel.Message = ClassBookConstantString.Edit_Student_Failed;
                        exceptionModel.ValidationMessage.Add(ClassBookConstantString.Validation_EmailExist);
                    }
                    else
                    {
                        var singleUser = _context.Student.Where(x => x.Id == 1).AsNoTracking().FirstOrDefault();
                        int studentId = _classBookService.UpdateStudent(studentData, singleUser, model.files);
                        exceptionModel.Status = false;
                        exceptionModel.Message = ClassBookConstantString.Edit_Student_Success;
                    }
                }
                return Ok(exceptionModel);
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Student, exception, "api/student/EditStudent", 0);
                exceptionModel.Status = false;
                exceptionModel.Message = ClassBookConstantString.Edit_Student_Failed.ToString();
                exceptionModel.ErrorMessage.Add(exception?.Message);
                exceptionModel.ErrorMessage.Add(exception?.InnerException?.Message);
                return Ok(exceptionModel);
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