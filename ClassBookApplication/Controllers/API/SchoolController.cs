using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.School;
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
    public class SchoolController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly LogsService _logsService;
        private readonly ClassBookModelFactory _classBookModelFactory;



        #endregion

        #region Ctor

        public SchoolController(ClassBookManagementContext context,
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

        #region Register School

        // POST api/School/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    School schoolData = JsonConvert.DeserializeObject<School>(model.data.ToString());
                    if (schoolData != null)
                    {
                        var singleUser = _context.School.Where(x => x.Email == schoolData.Email).AsNoTracking();
                        if (!singleUser.Any())
                        {
                            (int SchoolId, string uniqueNo) = _classBookService.SaveSchool(schoolData, model.files);
                            string UserName = schoolData.Name + uniqueNo;
                            _classBookService.SaveMappingData((int)Module.School, SchoolId, schoolData.MappingRequestModel);
                            var user = _classBookService.SaveUserData(SchoolId, Module.School, UserName, schoolData.Email, model.FCMId, model.DeviceId);
                            await Task.Run(() => _classBookService.SendVerificationLinkEmail(schoolData.Email, user.Password, Module.School.ToString()));
                            var exceptionModel = new
                            {
                                Message = ClassBookConstantString.Register_School_Success.ToString(),
                                Data = _classBookModelFactory.PrepareUserDetail(user)
                            };
                            return StatusCode((int)HttpStatusCode.OK, exceptionModel);
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
                    return StatusCode((int)HttpStatusCode.BadRequest);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
                }

            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_School, exception, "api/School/Register", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }

        // POST api/School/EditSchool
        [HttpPost("EditSchool")]
        public IActionResult EditSchool([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    School SchoolData = JsonConvert.DeserializeObject<School>(model.data.ToString());
                    if (SchoolData != null)
                    {
                        if (_context.School.Count(x => x.Email == SchoolData.Email && x.Id != SchoolData.Id) > 0)
                        {
                            var authorizeAccess = new
                            {
                                Message = ClassBookConstantString.Validation_EmailExist.ToString()
                            };
                            return StatusCode((int)HttpStatusCode.Conflict, authorizeAccess);
                        }
                        else
                        {
                            var singleClass = _context.School.Where(x => x.Id == SchoolData.Id).AsNoTracking().FirstOrDefault();
                            int classId = _classBookService.UpdateSchool(SchoolData, singleClass, model.files);
                            _classBookService.SaveMappingData((int)Module.School, classId, SchoolData.MappingRequestModel);
                            var exceptionModel = new
                            {
                                Message = ClassBookConstantString.Edit_School_Success.ToString(),
                            };
                            return StatusCode((int)HttpStatusCode.OK, exceptionModel);
                        }
                    }
                    return StatusCode((int)HttpStatusCode.BadRequest);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
                }

            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_School, exception, "api/School/EditSchool", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }

        #endregion

        #region GetSchoolDetails

        // GET api/School/GetAllSchool
        [HttpGet("GetAllSchool")]
        public IEnumerable<ListingModel> GetAllSchool()
        {
            return _classBookService.GetModuleDataByModuleId((int)Module.School);

        }

        // GET api/School/GetSchoolById/5
        [HttpGet("GetSchoolById/{id:int}")]
        public IEnumerable<School> GetSchoolById(int id)
        {
            var school = _context.School.Where(x => x.Id == id).AsEnumerable();
            return school;
        }
        #endregion
    }
}
