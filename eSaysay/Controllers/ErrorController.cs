using Microsoft.AspNetCore.Mvc;

namespace eSaysay.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            return statusCode switch
            {
                404 => View("404"),
                403 => View("403"),
                500 => View("500"),
                _ => View("Error")
            };
        }
    }
}
