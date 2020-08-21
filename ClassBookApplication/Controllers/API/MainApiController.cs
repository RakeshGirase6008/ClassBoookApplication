using Microsoft.AspNetCore.Mvc;

namespace ClassBookApplication.Controllers.API
{
    //[ServiceFilter(typeof(ControllerFilterExample))]
    //[Route("api/[controller]")]
    //[Route("api/{v:apiVersion}/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1")]
    [ApiVersion("2")]
    public class MainApiController : ControllerBase
    {
    }
}