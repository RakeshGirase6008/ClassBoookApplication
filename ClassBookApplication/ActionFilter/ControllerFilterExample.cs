using ClassBookApplication.DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace ClassBookApplication.ActionFilter
{
    public class ControllerFilterExample : IActionFilter
    {
        #region Fields

        private readonly ClassBookManagementContext _context;

        #endregion

        #region Ctor
        public ControllerFilterExample(ClassBookManagementContext context)
        {
            this._context = context;
        }

        #endregion

        #region Method
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var secretKey = _context.Settings.Where(x => x.Name == "ApplicationSetting.SecretKey").AsNoTracking().FirstOrDefault();
            StringValues authorizationToken;
            var status = context.HttpContext.Request.Headers.TryGetValue("Secret_Key", out authorizationToken);
            var mySring = authorizationToken.ToString();
            if (secretKey == null || mySring != secretKey.Value.ToString() || status == false)
            {
                var validationError = new
                {
                    Message = "Secret_Key is not Valid"
                };
                context.Result = new UnauthorizedObjectResult(validationError);
                return;
            }
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //our code after action executes
        }

        #endregion
    }
}