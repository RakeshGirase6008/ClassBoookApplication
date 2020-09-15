using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Student;
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

    public class StudentController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly ClassBookModelFactory _classBookModelFactory;


        #endregion

        #region Ctor

        public StudentController(ClassBookManagementContext context,
            ClassBookService classBookService,
            ClassBookModelFactory classBookModelFactory)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._classBookModelFactory = classBookModelFactory;
        }

        #endregion

        #region Register User

        // POST api/Student/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                Student studentData = JsonConvert.DeserializeObject<Student>(model.data.ToString());
                if (studentData != null)
                {
                    var singleUser = _context.Users.Where(x => x.Email == studentData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int studentId, string uniqueNo) = _classBookService.SaveStudent(studentData, model.files);
                        string UserName = studentData.FirstName + studentData.LastName + uniqueNo;
                        var user = _classBookService.SaveUserData(studentId, Module.Student, UserName, studentData.Email, model.FCMId, model.DeviceId);
                        await Task.Run(() => _classBookService.SendVerificationLinkEmail(studentData.Email, user.Password, Module.Student.ToString()));
                        responseModel.Message = ClassBookConstantString.Register_Student_Success.ToString();
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

        // POST api/Student/EditStudent
        [HttpPost("EditStudent")]
        public IActionResult EditStudent([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                Student studentData = JsonConvert.DeserializeObject<Student>(model.data.ToString());
                if (studentData != null)
                {
                    if (_context.Users.Count(x => x.Email == studentData.Email && x.UserId != studentData.Id) > 0)
                    {
                        responseModel.Message = ClassBookConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                    else
                    {
                        var singleUser = _context.Student.Where(x => x.Id == studentData.Id).AsNoTracking().FirstOrDefault();
                        int studentId = _classBookService.UpdateStudent(studentData, singleUser, model.files);
                        responseModel.Message = ClassBookConstantString.Edit_Student_Success.ToString();
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
        public object GetStudentById(int id)
        {
            var query = from stud in _context.Student
                        join board in _context.Board on stud.BoardId equals board.Id
                        join medium in _context.Medium on stud.MediumId equals medium.Id
                        join standard in _context.Standards on stud.StandardId equals standard.Id
                        join state in _context.States on stud.StateId equals state.Id
                        join city in _context.City on stud.CityId equals city.Id
                        join pincode in _context.Pincode on stud.Pincode equals pincode.Id
                        where stud.Id == id && stud.Active == true
                        orderby stud.Id
                        select new
                        {
                            FirstName = stud.FirstName,
                            LastName = stud.LastName,
                            Address = stud.Address,
                            Email = stud.Email,
                            Gender = stud.Gender,
                            DOB = stud.DOB,
                            ImageUrl = stud.ProfilePictureUrl,
                            StateName = state.Name,
                            CityName = city.Name,
                            Pincode = pincode.Name,
                            BoardName = board.Name,
                            MediumName = medium.Name,
                            StandardName = standard.Name,
                        };
            var students = query.FirstOrDefault();
            return students;
        }
        #endregion

        #region Student another record

        // GET api/Student/GetClassById/5
        [HttpGet("GetClassById/{id:int}")]
        public object GetClassById(int id)
        {
            var query = from classes in _context.Classes
                        join city in _context.City on classes.CityId equals city.Id
                        where classes.Id == id && classes.Active == true
                        orderby classes.Id
                        select new CommonDetailModel
                        {
                            Id = classes.Id,
                            Name = classes.Name,
                            CityName = city.Name,
                            IntroductionURL = classes.IntroductionURL
                        };
            var ClassData = query.FirstOrDefault();
            if (ClassData != null)
                ClassData.BoardMediumStandardModel = _classBookService.GetDetailById(ClassData.Id, (int)Module.Classes);
            return ClassData;
        }


        // GET api/Student/GetTeacherById/5
        [HttpGet("GetTeacherById/{id:int}")]
        public object GetTeacherById(int id)
        {
            var query = from teacher in _context.Teacher
                        join city in _context.City on teacher.CityId equals city.Id
                        where teacher.Id == id && teacher.Active == true
                        orderby teacher.Id
                        select new CommonDetailModel
                        {
                            Id = teacher.Id,
                            Name = teacher.FirstName + " " + teacher.LastName,
                            CityName = city.Name,
                            IntroductionURL = teacher.IntroductionURL
                        };
            var teacherData = query.FirstOrDefault();
            if(teacherData!=null)
                teacherData.BoardMediumStandardModel = _classBookService.GetDetailById(teacherData.Id,(int)Module.Teacher);
            return teacherData;
        }
        // POST api/Student/GetSubjects
        [HttpPost("GetSubjects")]
        public object GetSubjects([FromForm] SubjectRequestDetails subjectRequestDetails)
        {
            return _classBookService.GetSubjects(subjectRequestDetails);
        }
        #endregion
    }
}