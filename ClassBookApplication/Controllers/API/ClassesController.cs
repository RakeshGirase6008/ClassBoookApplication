using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.Classes;
using ClassBookApplication.Domain.Common;
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
    public class ClassesController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly LogsService _logsService;


        #endregion

        #region Ctor

        public ClassesController(ClassBookManagementContext context,
            ClassBookService classBookService,
            LogsService logsService)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._logsService = logsService;
        }

        #endregion

        #region Register Classes

        // POST api/Classes/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            CommonResponseModel exceptionModel = new CommonResponseModel();
            try
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
                        exceptionModel.Status = true;
                        exceptionModel.Message = ClassBookConstantString.Register_Classes_Success.ToString();
                    }
                    else
                    {
                        exceptionModel.Status = false;
                        exceptionModel.Message = ClassBookConstantString.Register_Classes_Failed.ToString();
                        exceptionModel.ValidationMessage.Add(ClassBookConstantString.Validation_EmailExist.ToString());
                    }
                }
                return Ok(exceptionModel);
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Classes, exception, "api/Classes/Register", 0);
                exceptionModel.Status = false;
                exceptionModel.Message = ClassBookConstantString.Register_Classes_Failed.ToString();
                exceptionModel.ErrorMessage.Add(exception?.Message);
                exceptionModel.ErrorMessage.Add(exception?.InnerException?.ToString());
                return Ok(exceptionModel);
            }
        }

        // POST api/Classes/EditClasses
        [HttpPost("EditClasses")]
        public IActionResult EditClasses([FromForm] CommonRegistrationModel model)
        {
            CommonResponseModel exceptionModel = new CommonResponseModel();
            try
            {
                Classes classesData = JsonConvert.DeserializeObject<Classes>(model.data.ToString());
                if (classesData != null)
                {
                    if (_context.Classes.Count(x => x.Email == classesData.Email && x.Id != classesData.Id) > 0)
                    {
                        exceptionModel.Status = false;
                        exceptionModel.Message = ClassBookConstantString.Edit_Classes_Failed;
                        exceptionModel.ValidationMessage.Add(ClassBookConstantString.Validation_EmailExist);
                    }
                    else
                    {
                        var singleClass = _context.Classes.Where(x => x.Id == classesData.Id).AsNoTracking().FirstOrDefault();
                        int classId = _classBookService.UpdateClasses(classesData, singleClass, model.files);
                        _classBookService.SaveMappingData((int)Module.Classes, classId, classesData.MappingRequestModel);
                        exceptionModel.Status = true;
                        exceptionModel.Message = ClassBookConstantString.Edit_Classes_Success;
                    }
                }
                return Ok(exceptionModel);
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_Classes, exception, "api/Classes/EditClasses", 0);
                exceptionModel.Status = false;
                exceptionModel.Message = ClassBookConstantString.Edit_Classes_Failed.ToString();
                exceptionModel.ErrorMessage.Add(exception?.Message);
                exceptionModel.ErrorMessage.Add(exception?.InnerException?.Message);
                return Ok(exceptionModel);
            }
        }

        #endregion

        #region GetClassesDetails

        // GET api/Classes/GetAllClasses
        [HttpGet("GetAllClasses")]
        public IEnumerable<ListingModel> GetAllClasses()
        {
            return _classBookService.GetAllClassesData();
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
