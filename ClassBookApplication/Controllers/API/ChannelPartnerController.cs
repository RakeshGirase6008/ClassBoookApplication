using ClassBookApplication.DataContext;
using ClassBookApplication.Domain.ChannelPartner;
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
    public class ChannelPartnerController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly ClassBookModelFactory _classBookModelFactory;


        #endregion

        #region Ctor

        public ChannelPartnerController(ClassBookManagementContext context,
            ClassBookService classBookService,
            ClassBookModelFactory classBookModelFactory)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._classBookModelFactory = classBookModelFactory;
        }

        #endregion

        #region Register ChannelPartner

        // POST api/ChannelPartner/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                ChannelPartner ChannelPartnerData = JsonConvert.DeserializeObject<ChannelPartner>(model.Data.ToString());
                if (ChannelPartnerData != null)
                {
                    var singleUser = _context.Users.Where(x => x.Email == ChannelPartnerData.Email).AsNoTracking();
                    if (!singleUser.Any())
                    {
                        (int channelPartnerId, string uniqueNo) = _classBookService.SaveChannelPartner(ChannelPartnerData, model.Files);
                        string UserName = ChannelPartnerData.FirstName + uniqueNo;
                        var user = _classBookService.SaveUserData(channelPartnerId, Module.ChannelPartner, UserName, ChannelPartnerData.Email, model.FCMId, model.DeviceId);
                        await Task.Run(() => _classBookService.SendVerificationLinkEmail(ChannelPartnerData.Email, user.Password, Module.ChannelPartner.ToString()));
                        responseModel.Message = ClassBookConstantString.Register_ChannelPartner_Success.ToString();
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

        // POST api/ChannelPartner/EditCareerExpert
        [HttpPost("EditChannelPartner")]
        public IActionResult EditChannelPartner([FromForm] CommonRegistrationModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                ChannelPartner ChannelPartnerData = JsonConvert.DeserializeObject<ChannelPartner>(model.Data.ToString());
                if (ChannelPartnerData != null)
                {
                    if (_context.Users.Count(x => x.Email == ChannelPartnerData.Email && x.EntityId != ChannelPartnerData.Id) > 0)
                    {
                        responseModel.Message = ClassBookConstantString.Validation_EmailExist.ToString();
                        return StatusCode((int)HttpStatusCode.Conflict, responseModel);
                    }
                    else
                    {
                        var singleChannelPartner = _context.ChannelPartner.Where(x => x.Id == ChannelPartnerData.Id).AsNoTracking().FirstOrDefault();
                        int channelPartnerId = _classBookService.UpdateChannelPartner(ChannelPartnerData, singleChannelPartner, model.Files);
                        responseModel.Message = ClassBookConstantString.Edit_ChannelPartner_Success.ToString();
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

        // GET api/ChannelPartner/GetAllChannelPartner
        [HttpGet("GetAllChannelPartner")]
        public IEnumerable<ListingModel> GetAllChannelPartner()
        {
            return _classBookService.GetModuleDataByModuleId((int)Module.CareerExpert);
        }

        // GET api/ChannelPartner/GetChannelPartnerById/5
        [HttpGet("GetChannelPartnerById/{id:int}")]
        public object GetChannelPartnerById(int id)
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
