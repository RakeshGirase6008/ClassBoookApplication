﻿using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Teacher;
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
    public class TeacherController : MainApiController
    {

        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
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
            this._classBookModelFactory = classBookModelFactory;
        }

        #endregion

        #region Register Teacher

        // POST api/Teacher/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                Teacher teacherData = JsonConvert.DeserializeObject<Teacher>(model.Data.ToString());
                if (teacherData != null)
                {
                    var singleUser = _context.Users.Where(x => x.Email == teacherData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int teacherId, string uniqueNo) = _classBookService.SaveTeacher(teacherData, model.Files, model.Video);
                        string UserName = teacherData.FirstName + uniqueNo;
                        //_classBookService.SaveMappingData((int)Module.Teacher, teacherId, teacherData.MappingRequestModel);
                        var user = _classBookService.SaveUserData(teacherId, Module.Teacher, UserName, teacherData.Email, model.FCMId, model.DeviceId);
                        var rest = _classBookService.RegisterMethod(model, "/api/v1/ChannelPartner/register");
                        await Task.Run(() => _classBookService.SendVerificationLinkEmail(teacherData.Email, user.Password, Module.Teacher.ToString()));
                        responseModel.Message = ClassBookConstantString.Register_Teacher_Success.ToString();
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

        // POST api/Teacher/EditTeacher
        [HttpPost("EditTeacher")]
        public IActionResult EditTeacher([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                Teacher teacherData = JsonConvert.DeserializeObject<Teacher>(model.Data.ToString());
                if (teacherData != null)
                {
                    if (_context.Users.Count(x => x.Email == teacherData.Email && x.EntityId != teacherData.Id) > 0)
                    {
                        responseModel.Message = ClassBookConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                    else
                    {
                        var singleTeacher = _context.Teacher.Where(x => x.Id == teacherData.Id).AsNoTracking().FirstOrDefault();
                        int teacherId = _classBookService.UpdateTeachers(teacherData, singleTeacher, model.Files);
                        //_classBookService.SaveMappingData((int)Module.Teacher, teacherId, teacherData.MappingRequestModel);
                        responseModel.Message = ClassBookConstantString.Edit_Teacher_Success.ToString();
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

        #region GetTeacherDetails

        // GET api/Teacher/GetAllTeacher
        [HttpGet("GetAllTeacher")]
        public IEnumerable<ListingModel> GetAllTeacher()
        {
            return _classBookService.GetModuleDataByModuleId((int)Module.Teacher);

        }

        // GET api/Teacher/GetTeacherById/5
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
                            IntroductionURL = _classBookModelFactory.PrepareURL(teacher.IntroductionURL)
                        };
            var teacherData = query.FirstOrDefault();
            if (teacherData != null)
                teacherData.BoardMediumStandardModel = _classBookService.GetDetailById(teacherData.Id, (int)Module.Teacher);
            return teacherData;
        }

        // GET api/Teacher/EditProfileForTeacher/5
        [HttpGet("EditProfileForTeacher/{id:int}")]
        public object EditProfileForTeacher(int id)
        {
            var query = from teacher in _context.Teacher
                        join state in _context.States on teacher.StateId equals state.Id
                        join city in _context.City on teacher.CityId equals city.Id
                        join pincode in _context.Pincode on teacher.Pincode equals pincode.Id
                        where teacher.Id == id && teacher.Active == true
                        orderby teacher.Id
                        select new
                        {
                            FirstName = teacher.FirstName,
                            LastName = teacher.LastName,
                            Email = teacher.Email,
                            ContactNo = teacher.ContactNo,
                            AlternateContact = teacher.AlternateContact,
                            Gender = teacher.Gender,
                            ProfilePictureUrl = teacher.ProfilePictureUrl,
                            DOB = teacher.DOB,
                            Address = teacher.Address,
                            TeachingExperience = teacher.TeachingExperience,
                            Description = teacher.Description,
                            ReferCode = teacher.ReferCode,
                            UniqueNo = teacher.UniqueNo,
                            StateName = state.Name,
                            CityName = city.Name,
                            Pincode = pincode.Name
                        };
            var teacherData = query.FirstOrDefault();
            return teacherData;
        }

        // GET api/Teacher/GetTeacherInformationByReferCode
        [HttpGet("GetTeacherInformationByReferCode")]
        public object GetTeacherInformationByReferCode(string referCode)
        {
            return _context.Teacher.Where(x => x.ReferCode == referCode).ToList();
        }
        #endregion
    }
}
