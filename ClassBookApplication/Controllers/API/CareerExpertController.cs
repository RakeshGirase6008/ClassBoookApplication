using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.CareerExpert;
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
    public class CareerExpertController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly LogsService _logsService;

        #endregion

        #region Ctor

        public CareerExpertController(ClassBookManagementContext context,
            ClassBookService classBookService,
            LogsService logsService)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._logsService = logsService;
        }

        #endregion

        #region Register CareerExpert

        // POST api/CareerExpert/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            CommonResponseModel exceptionModel = new CommonResponseModel();
            try
            {
                CareerExpert CareerExpertData = JsonConvert.DeserializeObject<CareerExpert>(model.data.ToString());
                if (CareerExpertData != null)
                {
                    var singleUser = _context.CareerExpert.Where(x => x.Email == CareerExpertData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int CareerExpertId, string uniqueNo) = _classBookService.SaveCareerExpert(CareerExpertData, model.files);
                        string UserName = CareerExpertData.FirstName + uniqueNo;
                        _classBookService.SaveMappingData((int)Module.CareerExpert, CareerExpertId, CareerExpertData.MappingRequestModel);
                        var user = _classBookService.SaveUserData(CareerExpertId, Module.CareerExpert, UserName, CareerExpertData.Email, model.FCMId, model.DeviceId);
                        await Task.Run(() => _classBookService.SendVerificationLinkEmail(CareerExpertData.Email, user.Password, Module.CareerExpert.ToString()));
                        exceptionModel.Status = true;
                        exceptionModel.Data = user;
                        exceptionModel.Message = ClassBookConstantString.Register_CareerExpert_Success.ToString();
                    }
                    else
                    {
                        exceptionModel.Status = false;
                        exceptionModel.Message = ClassBookConstantString.Register_CareerExpert_Failed.ToString();
                        exceptionModel.ValidationMessage.Add(ClassBookConstantString.Validation_EmailExist.ToString());
                    }
                }
                return Ok(exceptionModel);
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_CareerExpert, exception, "api/CareerExpert/Register", 0);
                exceptionModel.Status = false;
                exceptionModel.Message = ClassBookConstantString.Register_CareerExpert_Failed.ToString();
                exceptionModel.ErrorMessage.Add(exception?.Message);
                exceptionModel.ErrorMessage.Add(exception?.InnerException?.ToString());
                return Ok(exceptionModel);
            }
        }

        // POST api/CareerExpert/EditCareerExpert
        [HttpPost("EditCareerExpert")]
        public IActionResult EditCareerExpert([FromForm] CommonRegistrationModel model)
        {
            CommonResponseModel exceptionModel = new CommonResponseModel();
            try
            {
                CareerExpert CareerExpertData = JsonConvert.DeserializeObject<CareerExpert>(model.data.ToString());
                if (CareerExpertData != null)
                {
                    if (_context.CareerExpert.Count(x => x.Email == CareerExpertData.Email && x.Id != CareerExpertData.Id) > 0)
                    {
                        exceptionModel.Status = false;
                        exceptionModel.Message = ClassBookConstantString.Edit_CareerExpert_Failed;
                        exceptionModel.ValidationMessage.Add(ClassBookConstantString.Validation_EmailExist);
                    }
                    else
                    {
                        var singleCareerExpert = _context.CareerExpert.Where(x => x.Id == CareerExpertData.Id).AsNoTracking().FirstOrDefault();
                        int CareerExpertId = _classBookService.UpdateCareerExpert(CareerExpertData, singleCareerExpert, model.files);
                        _classBookService.SaveMappingData((int)Module.CareerExpert, CareerExpertId, CareerExpertData.MappingRequestModel);
                        exceptionModel.Status = true;
                        exceptionModel.Message = ClassBookConstantString.Edit_CareerExpert_Success;
                    }
                }
                return Ok(exceptionModel);
            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_CareerExpert, exception, "api/CareerExpert/EditCareerExpert", 0);
                exceptionModel.Status = false;
                exceptionModel.Message = ClassBookConstantString.Edit_CareerExpert_Failed.ToString();
                exceptionModel.ErrorMessage.Add(exception?.Message);
                exceptionModel.ErrorMessage.Add(exception?.InnerException?.Message);
                return Ok(exceptionModel);
            }
        }

        #endregion

        #region GetCareerExpertDetails

        // GET api/CareerExpert/GetAllCareerExpert
        [HttpGet("GetAllCareerExpert")]
        public IEnumerable<CareerExpert> GetAllCareerExpert()
        {
            var CareerExpert = _context.CareerExpert.Where(x => x.Active == true && x.Deleted == false).AsEnumerable();
            return CareerExpert;
        }

        // GET api/CareerExpert/GetClasById/5
        [HttpGet("GetClasById/{id:int}")]
        public IEnumerable<CareerExpert> GetClasById(int id)
        {
            var CareerExpert = _context.CareerExpert.Where(x => x.Id == id).AsEnumerable();
            return CareerExpert;
        }
        #endregion
    }
}
