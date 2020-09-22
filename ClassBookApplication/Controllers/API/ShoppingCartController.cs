using ClassBookApplication.DataContext;
using ClassBookApplication.Models.RequestModels;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ClassBookApplication.Controllers.API
{
    [ApiVersion("1")]
    public class ShoppingCartController : MainApiController
    {
        #region Fields

        private readonly ClassBookManagementContext _context;
        private readonly ClassBookService _classBookService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        #endregion

        #region Ctor

        public ShoppingCartController(ClassBookManagementContext context,
            ClassBookService classBookService,
            IHttpContextAccessor httpContextAccessor)
        {
            this._context = context;
            this._classBookService = classBookService;
            this._httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region ShoppingCart

        // POST api/ShoppingCart/AddToCart
        [HttpPost("AddToCart")]
        public IActionResult AddToCart([FromForm] AddToCartModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
                var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).AsNoTracking();
                if (singleUser.Any())
                {
                    var user = singleUser.FirstOrDefault();
                    var message = _classBookService.SaveShoppingCart(user.Id, model);
                    responseModel.Message = message;
                    return StatusCode((int)HttpStatusCode.OK, responseModel);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, responseModel);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }
        }

        // POST api/ShoppingCart/RemoveFromCart
        [HttpPost("RemoveFromCart")]
        public IActionResult RemoveFromCart([FromForm] AddToCartModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
                var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).AsNoTracking();
                if (singleUser.Any())
                {
                    var user = singleUser.FirstOrDefault();
                    var message = _classBookService.SaveShoppingCart(user.Id, model, true);
                    responseModel.Message = message;
                    return StatusCode((int)HttpStatusCode.OK, responseModel);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, responseModel);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }
        }


        // POST api/ShoppingCart/GetCartDetail
        [HttpPost("GetCartDetail")]
        public IEnumerable<CartDetailModel> GetCartDetail()
        {
            string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
            var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).FirstOrDefault();
            return _classBookService.GetCartDetailByUserId(singleUser.Id, singleUser.ModuleId);
        }


        // POST api/ShoppingCart/CompleteOrder
        [HttpPost("CompleteOrder")]
        public bool CompleteOrder([FromForm] string PaymentType)
        {
            string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
            var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).FirstOrDefault();
            var status = _classBookService.OrderPaid(singleUser.Id, singleUser.ModuleId, PaymentType);
            return status;
        }

        #endregion

        #region ClassTeacher ShoppingCart

        // POST api/ShoppingCart/ClassTeacherAddToCart
        [HttpPost("ClassTeacherAddToCart")]
        public IActionResult ClassTeacherAddToCart([FromForm] AddToCartModelClassTeacher model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
                var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).AsNoTracking();
                if (singleUser.Any())
                {
                    var user = singleUser.FirstOrDefault();
                    var message = _classBookService.SaveShoppingCartClassTeacher(user.Id, model);
                    responseModel.Message = message;
                    return StatusCode((int)HttpStatusCode.OK, responseModel);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, responseModel);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }
        }

        // POST api/ShoppingCart/ClassTeacherRemoveFromCart
        [HttpPost("ClassTeacherRemoveFromCart")]
        public IActionResult ClassTeacherRemoveFromCart([FromForm] AddToCartModel model)
        {
            ResponseModel responseModel = new ResponseModel();
            if (ModelState.IsValid)
            {
                string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
                var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).AsNoTracking();
                if (singleUser.Any())
                {
                    var user = singleUser.FirstOrDefault();
                    var message = _classBookService.SaveShoppingCart(user.Id, model, true);
                    responseModel.Message = message;
                    return StatusCode((int)HttpStatusCode.OK, responseModel);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.Unauthorized, responseModel);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ModelState);
            }
        }


        // POST api/ShoppingCart/ClassTeacherGetCartDetail
        [HttpPost("ClassTeacherGetCartDetail")]
        public IEnumerable<CartDetailModel> ClassTeacherGetCartDetail()
        {
            string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
            var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).FirstOrDefault();
            return _classBookService.GetCartDetailByUserId(singleUser.Id, singleUser.ModuleId);
        }


        // POST api/ShoppingCart/ClassTeacherCompleteOrder
        [HttpPost("ClassTeacherCompleteOrder")]
        public bool ClassTeacherCompleteOrder([FromForm] string PaymentType)
        {
            string authorizeTokenKey = _httpContextAccessor.HttpContext.Request.Headers["AuthorizeTokenKey"];
            var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizeTokenKey).FirstOrDefault();
            var status = _classBookService.OrderPaid(singleUser.Id, singleUser.ModuleId, PaymentType);
            return status;
        }

#endregion
    }
}
