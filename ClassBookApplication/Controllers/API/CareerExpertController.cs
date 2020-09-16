using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.CareerExpert;
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
    public class CareerExpertController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly ClassBookModelFactory _classBookModelFactory;


        #endregion

        #region Ctor

        public CareerExpertController(ClassBookManagementContext context,
            ClassBookService classBookService,
            ClassBookModelFactory classBookModelFactory)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._classBookModelFactory = classBookModelFactory;
        }

        #endregion

        #region Register CareerExpert

        // POST api/CareerExpert/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                CareerExpert CareerExpertData = JsonConvert.DeserializeObject<CareerExpert>(model.Data.ToString());
                if (CareerExpertData != null)
                {
                    var singleUser = _context.Users.Where(x => x.Email == CareerExpertData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int CareerExpertId, string uniqueNo) = _classBookService.SaveCareerExpert(CareerExpertData, model.Files);
                        string UserName = CareerExpertData.FirstName + uniqueNo;
                        //_classBookService.SaveMappingData((int)Module.CareerExpert, CareerExpertId, CareerExpertData.MappingRequestModel);
                        var user = _classBookService.SaveUserData(CareerExpertId, Module.CareerExpert, UserName, CareerExpertData.Email, model.FCMId, model.DeviceId);
                        await Task.Run(() => _classBookService.SendVerificationLinkEmail(CareerExpertData.Email, user.Password, Module.CareerExpert.ToString()));
                        responseModel.Message = ClassBookConstantString.Register_CareerExpert_Success.ToString();
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

        // POST api/CareerExpert/EditCareerExpert
        [HttpPost("EditCareerExpert")]
        public IActionResult EditCareerExpert([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                CareerExpert CareerExpertData = JsonConvert.DeserializeObject<CareerExpert>(model.Data.ToString());
                if (CareerExpertData != null)
                {
                    if (_context.Users.Count(x => x.Email == CareerExpertData.Email && x.UserId != CareerExpertData.Id) > 0)
                    {
                        responseModel.Message = ClassBookConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                    else
                    {
                        var singleCareerExpert = _context.CareerExpert.Where(x => x.Id == CareerExpertData.Id).AsNoTracking().FirstOrDefault();
                        int CareerExpertId = _classBookService.UpdateCareerExpert(CareerExpertData, singleCareerExpert, model.Files);
                        //_classBookService.SaveMappingData((int)Module.CareerExpert, CareerExpertId, CareerExpertData.MappingRequestModel);
                        responseModel.Message = ClassBookConstantString.Edit_CareerExpert_Success.ToString();
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

        #region GetCareerExpertDetails

        // GET api/CareerExpert/GetAllCareerExpert
        [HttpGet("GetAllCareerExpert")]
        public IEnumerable<ListingModel> GetAllCareerExpert()
        {
            return _classBookService.GetModuleDataByModuleId((int)Module.CareerExpert);
        }

        // GET api/CareerExpert/GetCareerExpertById/5
        [HttpGet("GetCareerExpertById/{id:int}")]
        public object GetCareerExpertById(int id)
        {
            var query = from careerExpert in _context.CareerExpert
                        join state in _context.States on careerExpert.StateId equals state.Id
                        join city in _context.City on careerExpert.CityId equals city.Id
                        join pincode in _context.Pincode on careerExpert.Pincode equals pincode.Id
                        where careerExpert.Id == id && careerExpert.Active == true
                        orderby careerExpert.Id
                        select new
                        {
                            FirstName = careerExpert.FirstName,
                            LastName = careerExpert.LastName,
                            Address = careerExpert.Address,
                            Email = careerExpert.Email,
                            Gender = careerExpert.Gender,
                            ImageUrl = careerExpert.ProfilePictureUrl,
                            DOB = careerExpert.DOB,
                            ContactNo = careerExpert.ContactNo,
                            AlternateContact = careerExpert.AlternateContact,
                            TeachingExperience = careerExpert.TeachingExperience,
                            Description = careerExpert.Description,
                            ReferCode = careerExpert.ReferCode,
                            StateName = state.Name,
                            CityName = city.Name,
                            Pincode = pincode.Name,
                        };
            var careerExpertData = query.FirstOrDefault();
            return careerExpertData;
        }
        #endregion
    }
}
