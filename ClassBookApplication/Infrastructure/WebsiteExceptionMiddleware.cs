using ClassBookApplication.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Threading.Tasks;

namespace ClassBookApplication.Infrastructure
{
    public class WebsiteExceptionMiddleware
    {
        #region Fields

        private readonly RequestDelegate _next;
        private readonly LogsService _logsService;

        #endregion

        #region Ctor

        public WebsiteExceptionMiddleware(RequestDelegate next, LogsService logsService)
        {
            _next = next;
            _logsService = logsService;
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
                    var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                    if (controllerActionDescriptor != null)
                        controllerName = controllerActionDescriptor.ControllerName;
                }
                _logsService.InsertLogs(controllerName, ex, httpContext.Request.Path.Value, userId);
                await HandleExceptionAsync(httpContext);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context)
        {
            // This will not cause any Issue.
            context.Response.StatusCode = 500;
            context.Response.Redirect("/Home/Error");
        }

        #endregion
    }
}
