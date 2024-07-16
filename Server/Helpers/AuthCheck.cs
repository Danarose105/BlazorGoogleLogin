using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace BlazorGoogleLogin.Helpers
{
    public class AuthCheck : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var token = context.HttpContext.Request.Cookies["jwt_token"];
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var tokenService = context.HttpContext.RequestServices.GetService<TokenService>();
            var principal = tokenService?.ValidateToken(token);
            if (principal == null)
            {
                context.HttpContext.Response.Cookies.Delete("jwt_token");
                context.Result = new UnauthorizedResult();
                return;
            }

            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                context.HttpContext.Response.Cookies.Delete("jwt_token");
                context.Result = new UnauthorizedResult();
                return;
            }

            // Refresh token if needed
            var refreshedToken = tokenService.RefreshToken(token);
            if (refreshedToken != token)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.Now.AddDays(1)
                };
                context.HttpContext.Response.Cookies.Append("jwt_token", refreshedToken, cookieOptions);
                token = refreshedToken;
            }

            var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            context.ActionArguments["token"] = token;

            await next();
        }
    }
}
