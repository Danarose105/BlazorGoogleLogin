using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;
using System.Net.Http.Json;
using BlazorGoogleLogin.Client.Helpers;
using System.Net.Http;
using System.Security.Claims;
using BlazorGoogleLogin.Shared.DTOs;

namespace BlazorGoogleLogin.Client.Helpers
{
    public class AuthenticationService : IAuthenticationService
    {

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;

        public AuthenticationService(HttpClient client, AuthenticationStateProvider authStateProvider, ILocalStorageService localStorage)
        {
            _httpClient = client;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }
        public async Task Logout()
        {

            var get = await _httpClient.GetAsync("api/users/logout");

            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();

        }
        public async Task<UserDTO> GetUserFromClaimAsync()
        {
            var authenticationState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authenticationState.User;

            if (user.Identity.IsAuthenticated)
            {
                var userDto = new UserDTO
                {
                    firstName = user.FindFirst(c => c.Type == "name")?.Value,
                    Mail = user.FindFirst(c => c.Type == "email")?.Value,
                    Id = Convert.ToInt16(user.FindFirst(c => c.Type == "sub")?.Value)
                    // Map other claims to UserDTO properties as needed
                };

                return userDto;
            }
            else
            {
                return null; // Or throw an exception, or handle unauthenticated user as needed
            }
        }


    }
}
