using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace EmploymentApp.Viewmodels
{
    public class ApplicantProfileResponse
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("middle_name")]
        public string MiddleName { get; set; }

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("birth_date")]
        public string BirthDate { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("about")]
        public string About { get; set; }

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; }
    }

    public partial class ApplicantViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string firstName = "";

        [ObservableProperty]
        private string lastName = "";

        [ObservableProperty]
        private string middleName = "";

        [ObservableProperty]
        private string phoneNumber = "";

        [ObservableProperty]
        private string birthDate = "";

        [ObservableProperty]
        private string city = "";

        [ObservableProperty]
        private string about = "";

        [ObservableProperty]
        private string skillsText = "";

        [ObservableProperty]
        private bool isLoading = false;

        public ApplicantViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
            LoadProfileAsync();
        }

        private async void LoadProfileAsync()
        {
            await LoadProfile();
        }

        [RelayCommand]
        private async Task LoadProfile()
        {
            IsLoading = true;

            try
            {
                var token = await _authService.GetAccessTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        "Токен не найден",
                        "OK"
                    );
                    IsLoading = false;
                    return;
                }

                var profileData = await _apiClient.GetAsJsonAsync<ApplicantProfileResponse>(
                    "/profile/me",
                    token
                );

                if (profileData != null)
                {
                    FirstName = profileData.FirstName ?? "";
                    LastName = profileData.LastName ?? "";
                    MiddleName = profileData.MiddleName ?? "";
                    PhoneNumber = profileData.PhoneNumber ?? "";
                    BirthDate = profileData.BirthDate ?? "";
                    City = profileData.City ?? "";
                    About = profileData.About ?? "";

                    if (profileData.Skills != null && profileData.Skills.Count > 0)
                    {
                        SkillsText = string.Join(", ", profileData.Skills);
                    }

                    Debug.WriteLine("Profile loaded successfully");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        "Не удалось загрузить профиль",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load profile error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    $"Ошибка загрузки профиля: {ex.Message}",
                    "OK"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}