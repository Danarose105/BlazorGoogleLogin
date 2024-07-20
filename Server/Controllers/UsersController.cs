using BlazorGoogleLogin.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace BlazorGoogleLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(AuthCheck))]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> GetLoginUser(string token)
        {
            return Ok(token);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            // Remove the JWT token cookie
            Response.Cookies.Delete("jwt_token");

            // Optionally, you can add additional sign-out logic here

            return Redirect("./../../");
        }


    }
}
