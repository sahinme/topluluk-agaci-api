using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Nnn.Web.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected string GetToken()
        {
            string value = Request.Headers[HeaderNames.Authorization];
            if (value == "Bearer null" || value=="Bearer undefined"  || value == null )
            {
                return "";
            }
            return value.Replace("Bearer ","");
        }
    }
}
