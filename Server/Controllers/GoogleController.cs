using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BlazorGoogleLogin.Helpers;
using System.IdentityModel.Tokens.Jwt;
using BlazorGoogleLogin.Shared.DTOs;
using BlazorGoogleLogin.Data;
using Microsoft.AspNetCore.Identity;

namespace BlazorGoogleLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoogleController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly DbRepository _db;

        public GoogleController(TokenService tokenService, DbRepository db)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {

            string finalUrl = "../../";



            var props = new AuthenticationProperties { RedirectUri = "api/google/signin-google" };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet("signin-google")]
        public async Task<IActionResult> GoogleLogin()
        {

            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (result?.Principal == null)
                return BadRequest();
            GoogleUserDTO googleUser = new GoogleUserDTO()
            {  
                Mail = result.Principal.FindFirstValue(ClaimTypes.Email),
                firstName = result.Principal.FindFirstValue(ClaimTypes.Name),
                GoogleID = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            string getUserQuery = "SELECT * FROM users WHERE GoogleID=@GoogleID";
            UserDTO currentUser = (await _db.GetRecordsAsync<UserDTO>(getUserQuery, googleUser)).FirstOrDefault();
            if (currentUser == null)
            {
                string insertUserQuery = "INSERT INTO users (mail,firstName,GoogleID) VALUES (@mail,@firstName,@GoogleID)";
                int id = await _db.InsertReturnIdAsync(insertUserQuery, googleUser);
                if (id == 0)
                {
                    return BadRequest("something happend");
                }
                currentUser = new UserDTO()
                {
                    Id = id,
                    firstName = googleUser.firstName,
                    Mail = googleUser.Mail
                };
            }


            // Create JWT token with additional claims
            var claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, currentUser.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, currentUser.Mail),
                    new Claim(JwtRegisteredClaimNames.Name, currentUser.firstName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
              };

            var token = _tokenService.GenerateToken(claims);

            // Set the token in a secure HTTP-only cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.Now.AddDays(1)
                //Expires =null
            };
            Response.Cookies.Append("jwt_token", token, cookieOptions);
            Console.WriteLine("hihihih");

            return Redirect("./../../");
        }

      
    }

    public class GoogleUserDTO
    {
        public string Mail { get; set; }
        public string firstName { get; set; }
        public string GoogleID { get; set; }
    }

}
