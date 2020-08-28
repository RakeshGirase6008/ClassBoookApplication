﻿using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.CareerExpert;
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
    public class CareerExpertController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly LogsService _logsService;
        private readonly ClassBookModelFactory _classBookModelFactory;


        #endregion

        #region Ctor

        public CareerExpertController(ClassBookManagementContext context,
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

        #region Register CareerExpert

        // POST api/CareerExpert/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
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
                            var exceptionModel = new
                            {
                                Message = ClassBookConstantString.Register_CareerExpert_Success.ToString(),
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
                    return Ok();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
                }

            }
            catch (Exception exception)
            {
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_CareerExpert, exception, "api/CareerExpert/Register", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }

        // POST api/CareerExpert/EditCareerExpert
        [HttpPost("EditCareerExpert")]
        public IActionResult EditCareerExpert([FromForm] CommonRegistrationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CareerExpert CareerExpertData = JsonConvert.DeserializeObject<CareerExpert>(model.data.ToString());
                    if (CareerExpertData != null)
                    {
                        if (_context.CareerExpert.Count(x => x.Email == CareerExpertData.Email && x.Id != CareerExpertData.Id) > 0)
                        {
                            var authorizeAccess = new
                            {
                                Message = ClassBookConstantString.Validation_EmailExist.ToString()
                            };
                            return StatusCode((int)HttpStatusCode.Conflict, authorizeAccess);
                        }
                        else
                        {
                            var singleCareerExpert = _context.CareerExpert.Where(x => x.Id == CareerExpertData.Id).AsNoTracking().FirstOrDefault();
                            int CareerExpertId = _classBookService.UpdateCareerExpert(CareerExpertData, singleCareerExpert, model.files);
                            _classBookService.SaveMappingData((int)Module.CareerExpert, CareerExpertId, CareerExpertData.MappingRequestModel);
                            var exceptionModel = new
                            {
                                Message = ClassBookConstantString.Edit_CareerExpert_Success.ToString(),
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
                _logsService.InsertLogs(ClassBookConstant.LogLevelModule_CareerExpert, exception, "api/CareerExpert/EditCareerExpert", 0);
                return StatusCode((int)HttpStatusCode.InternalServerError, exception?.Message);
            }
        }

        #endregion

        #region GetCareerExpertDetails

        // GET api/CareerExpert/GetAllCareerExpert
        [HttpGet("GetAllCareerExpert")]
        public IEnumerable<ListingModel> GetAllCareerExpert()
        {
            var careerExpertsData = from careerExpert in _context.CareerExpert
                                    where careerExpert.Active == true
                                    select new ListingModel
                                    {
                                        Id = careerExpert.Id,
                                        Title = careerExpert.FirstName + " " + careerExpert.LastName,
                                        Image = "https://classbookapplication.appspot.com/" + careerExpert.ProfilePictureUrl.Replace("\\", "/"),
                                        Rating = 0,
                                        TotalBoard = 2,
                                        TotalStandard = 3,
                                        TotalSubject = 4
                                    };
            return careerExpertsData;
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
