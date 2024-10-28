//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace Contractors.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ProtectedController : ControllerBase
//    {
//        [HttpGet]
//        [Authorize(Roles = "Client")]
//        public IActionResult Get()
//        {
//            var request = Request;
//            return Ok("This is a protected resource for clients.");
//        }
//    }
//}
