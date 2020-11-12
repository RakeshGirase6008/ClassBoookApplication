using ClassBookApplication.DataContext;
using ClassBookApplication.Models.ResponseModel;
using ClassBookApplication.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ClassBookApplication.Infrastructure
{
    public class ExceptionMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;
        private readonly LogsService _logsService;
        private readonly ClassBookManagementContext _context;

        #endregion

        #region Ctor

        public ExceptionMiddleware(RequestDelegate next, LogsService logsService,
            ClassBookManagementContext context)
        {
            _next = next;
            _logsService = logsService;
            _context = context;
        }

        #endregion

        #region Method
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                var endpoint = httpContext.GetEndpoint();
                int userId = 0;
                string controllerName = string.Empty;
                if (endpoint != null)
                {
                    StringValues secretKeyToken;
                    httpContext.Request.Headers.TryGetValue("AuthorizeTokenKey", out secretKeyToken).ToString();
                    var authorizationTokenKey = secretKeyToken.ToString();
                    var singleUser = _context.Users.Where(x => x.AuthorizeTokenKey == authorizationTokenKey).AsNoTracking();
                    if (singleUser.Any())
                    {
                        userId = singleUser.FirstOrDefault().Id;
                    }
                    var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                    if (controllerActionDescriptor != null)
                        controllerName = controllerActionDescriptor.ControllerName;
                }
                _logsService.InsertLogs(controllerName, ex, httpContext.Request.Path.Value, userId);
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = exception?.Message
            }.ToString());

            //if (context.Request.Path.Value.Contains("api"))
            //{
            //}
            //context.Response.Redirect("/Home/Errorpage");
            //if (context.Request.Path.Value.ToLower() == "")
            //{
            //}
            ////return context.Response.WriteAsync(new ErrorDetails()
            ////{
            ////    StatusCode = context.Response.StatusCode,
            ////    Message = exception?.Message
            ////}.ToString());
            //return await context.Response.Redirect("/Home/Errorpage");
        }

        #endregion
    }
}