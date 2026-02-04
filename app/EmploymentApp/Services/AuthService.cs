using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;

namespace EmploymentApp.Services
{
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }

    public class AuthResponse
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class AuthService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:8000/api";

        private const string TokenKey = "access_token";
        private const string RefreshTokenKey = "refresh_token";
        private const string UserIdKey = "user_id";
        private const string UserRoleKey = "user_role";

        public event EventHandler<bool> AuthenticationChanged;

        public AuthService()
        {
            var handler = new HttpClientHandler();
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<(bool success, string userId)> RegisterAsync(
            string email,
            string password,
            string role)
        {
            try
            {
                var request = new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    Role = role
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{BaseUrl}/auth/register",
                    request
                );

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    await SaveTokensAsync(result);
                    AuthenticationChanged?.Invoke(this, true);
                    return (true, result.Id);
                }

                return (false, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<LoginResult> LoginAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{BaseUrl}/auth/login",
                    request
                );

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    await SaveTokensAsync(result);
                    AuthenticationChanged?.Invoke(this, true);

                    var role = ExtractRoleFromToken(result.AccessToken);
                    var userId = ExtractUserIdFromToken(result.AccessToken);

                    return new LoginResult
                    {
                        Success = true,
                        UserId = userId,
                        Role = role
                    };
                }

                return new LoginResult { Success = false };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                return new LoginResult { Success = false };
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            return token ?? string.Empty;
        }

        public async Task<string> GetUserIdAsync()
        {
            var userId = await SecureStorage.GetAsync(UserIdKey);
            return userId ?? string.Empty;
        }

        public async Task<string> GetUserRoleAsync()
        {
            var role = await SecureStorage.GetAsync(UserRoleKey);
            return role ?? string.Empty;
        }

        public async Task<UserAuthData> GetUserDataAsync()
        {
            return new UserAuthData
            {
                UserId = await GetUserIdAsync(),
                Role = await GetUserRoleAsync(),
                Token = await GetAccessTokenAsync(),
                IsAuthenticated = await IsAuthenticatedAsync()
            };
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            return !string.IsNullOrEmpty(token);
        }

        public async Task LogoutAsync()
        {
            try
            {
                SecureStorage.Remove(TokenKey);
                SecureStorage.Remove(RefreshTokenKey);
                SecureStorage.Remove(UserIdKey);
                SecureStorage.Remove(UserRoleKey);

                AuthenticationChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logout error: {ex.Message}");
            }
        }

        private async Task SaveTokensAsync(AuthResponse response)
        {
            try
            {
                if (response?.AccessToken != null)
                {
                    await SecureStorage.SetAsync(TokenKey, response.AccessToken);
                    Debug.WriteLine("Access token saved");
                }

                if (response?.RefreshToken != null)
                {
                    await SecureStorage.SetAsync(RefreshTokenKey, response.RefreshToken);
                    Debug.WriteLine("Refresh token saved");
                }

                var roleFromToken = ExtractRoleFromToken(response.AccessToken);
                if (!string.IsNullOrEmpty(roleFromToken))
                {
                    await SecureStorage.SetAsync(UserRoleKey, roleFromToken);
                    Debug.WriteLine($"User role saved from token: {roleFromToken}");
                }

                var userIdFromToken = ExtractUserIdFromToken(response.AccessToken);
                if (!string.IsNullOrEmpty(userIdFromToken))
                {
                    await SecureStorage.SetAsync(UserIdKey, userIdFromToken);
                    Debug.WriteLine($"User role saved from token: {userIdFromToken}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving tokens: {ex.Message}");
            }
        }

        private string ExtractRoleFromToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return string.Empty;

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");

                if (roleClaim != null)
                {
                    Debug.WriteLine($"Role extracted from token: {roleClaim.Value}");
                    return roleClaim.Value;
                }

                Debug.WriteLine("Role claim not found in token");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extracting role from token: {ex.Message}");
                return string.Empty;
            }
        }

        private string ExtractUserIdFromToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return string.Empty;

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var user_idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_id");

                if (user_idClaim != null)
                {
                    Debug.WriteLine($"Id extracted from token: {user_idClaim.Value}");
                    return user_idClaim.Value;
                }

                Debug.WriteLine("Id claim not found in token");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extracting Id from token: {ex.Message}");
                return string.Empty;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
    }

    public class UserAuthData
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}