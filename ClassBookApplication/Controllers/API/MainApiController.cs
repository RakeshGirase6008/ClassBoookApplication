using ClassBookApplication.ActionFilter;
using Microsoft.AspNetCore.Mvc;

namespace ClassBookApplication.Controllers.API
{
    [ServiceFilter(typeof(ControllerFilterExample))]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    public class MainApiController : ControllerBase
    {
    }
}