using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace ClassBookApplication.ActionFilter
{
    public class ControllerFilterExample : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            StringValues authorizationToken;
            var status = context.HttpContext.Request.Headers.TryGetValue("Authorization", out authorizationToken);
            var mySring = authorizationToken.ToString();
            if (mySring != "123" || status == false)
            {
                context.HttpContext.Response.StatusCode = 401;
                context.HttpContext.Abort();
            }
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //our code after action executes
        }
    }
}
