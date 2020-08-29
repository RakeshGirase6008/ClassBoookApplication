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
using System;
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
        private readonly LogsService _logsService;
        private readonly ClassBookModelFactory _classBookModelFactory;

        #endregion

        #region Ctor

        public ClassesController(ClassBookManagementContext context,
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

        #region Register Classes

        // POST api/Classes/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Classes classesData = JsonConvert.DeserializeObject<Classes>(model.data.ToString());
                    if (classesData != null)
                    {
                        var singleUser = _context.Classes.Where(x => x.Email == classesData.Email).AsNoTracking();
                        if (!singleUser.Any())
                        {
                            (int classesId, string uniqueNo) = _classBookService.SaveClasses(classesData, model.files);
                            string UserName = classesData.Name + uniqueNo;
                            _classBookService.SaveMappingData((int)Module.Classes, classesId, classesData.MappingRequestModel);
                            var user = _classBookService.SaveUserData(classesId, Module.Classes, UserName, classesData.Email, model.FCMId, model.DeviceId);
                            await Task.Run(() => _classBookService.SendVerificationLinkEmail(classesData.Email, user.Password, Module.Classes.ToString()));
                            var exceptionModel = new
                            {
                                Message = ClassBookConstantString.Login_Success.ToString(),
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
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Classes, exception, "api/Classes/Register", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }

        // POST api/Classes/EditClasses
        [HttpPost("EditClasses")]
        public IActionResult EditClasses([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Classes classesData = JsonConvert.DeserializeObject<Classes>(model.data.ToString());
                    if (classesData != null)
                    {
                        if (_context.Classes.Count(x => x.Email == classesData.Email && x.Id != classesData.Id) > 0)
                        {
                            var authorizeAccess = new
                            {
                                Message = ClassBookConstantString.Validation_EmailExist.ToString()
                            };
                            return StatusCode((int)HttpStatusCode.Conflict, authorizeAccess);
                        }
                        else
                        {
                            var singleClass = _context.Classes.Where(x => x.Id == classesData.Id).AsNoTracking().FirstOrDefault();
                            int classId = _classBookService.UpdateClasses(classesData, singleClass, model.files);
                            _classBookService.SaveMappingData((int)Module.Classes, classId, classesData.MappingRequestModel);
                            var exceptionModel = new
                            {
                                Message = ClassBookConstantString.Edit_Classes_Success.ToString(),
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
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Classes, exception, "api/Classes/EditClasses", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
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

        // GET api/Classes/GetClasById/5
        [HttpGet("GetClasById/{id:int}")]
        public IEnumerable<Classes> GetClasById(int id)
        {
            var classes = _context.Classes.Where(x => x.Id == id).AsEnumerable();
            return classes;
        }
        #endregion
    }
}
