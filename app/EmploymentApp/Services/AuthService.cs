using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

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

        // КЛЮЧИ для SecureStorage - все хранится безопасно
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

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
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

        /// <summary>
        /// Вход пользователя с сохранением ID и Role
        /// </summary>
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

                    // Возвращаем все нужные данные
                    return new LoginResult
                    {
                        Success = true,
                        UserId = result.Id,
                        Role = result.Role,
                        Email = result.Email
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

        /// <summary>
        /// Получить access token
        /// </summary>
        public async Task<string> GetAccessTokenAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            return token ?? string.Empty;
        }

        /// <summary>
        /// Получить ID пользователя
        /// </summary>
        public async Task<string> GetUserIdAsync()
        {
            var userId = await SecureStorage.GetAsync(UserIdKey);
            return userId ?? string.Empty;
        }

        /// <summary>
        /// Получить Role пользователя (applicant или employer)
        /// </summary>
        public async Task<string> GetUserRoleAsync()
        {
            var role = await SecureStorage.GetAsync(UserRoleKey);
            return role ?? string.Empty;
        }

        /// <summary>
        /// Получить ВСЕ данные пользователя сразу
        /// </summary>
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

        /// <summary>
        /// Проверить, авторизован ли пользователь
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await SecureStorage.GetAsync(TokenKey);
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// Выход (удаление всех данных из SecureStorage)
        /// </summary>
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

        /// <summary>
        /// Сохранить токены, ID и Role в безопасное хранилище
        /// </summary>
        private async Task SaveTokensAsync(AuthResponse response)
        {
            try
            {
                // Сохраняем ACCESS TOKEN
                if (response?.AccessToken != null)
                {
                    await SecureStorage.SetAsync(TokenKey, response.AccessToken);
                    Debug.WriteLine("Access token сохранён");
                }

                // Сохраняем REFRESH TOKEN
                if (response?.RefreshToken != null)
                {
                    await SecureStorage.SetAsync(RefreshTokenKey, response.RefreshToken);
                    Debug.WriteLine("Refresh token сохранён");
                }

                // Сохраняем USER ID 
                if (response?.Id != null)
                {
                    await SecureStorage.SetAsync(UserIdKey, response.Id);
                    Debug.WriteLine($"User ID сохранён: {response.Id}");
                }

                // Сохраняем ROLE 
                if (response?.Role != null)
                {
                    await SecureStorage.SetAsync(UserRoleKey, response.Role);
                    Debug.WriteLine($"User role сохранён: {response.Role}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving tokens: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // ============== ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ ==============

    /// <summary>
    /// Результат входа с ID и Role
    /// </summary>
    public class LoginResult
    {
        public bool Success { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }

    /// <summary>
    /// Все данные аутентифицированного пользователя
    /// </summary>
    public class UserAuthData
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}