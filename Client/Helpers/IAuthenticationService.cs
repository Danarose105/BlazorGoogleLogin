using BlazorGoogleLogin.Shared.DTOs;

namespace BlazorGoogleLogin.Client.Helpers
{
    public interface IAuthenticationService
    {
        Task Logout();
        Task<UserDTO> GetUserFromClaimAsync();
    }
}
