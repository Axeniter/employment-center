using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Collections.ObjectModel;
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

    public class VacancyTitleResponse
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    public class VacancyResponseItem
    {
        [JsonPropertyName("vacancy_id")]
        public int VacancyId { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("applicant_id")]
        public string ApplicantId { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
    }

    public class VacancyResponseViewModel : ObservableObject
    {
        public int VacancyId { get; set; }
        public int Id { get; set; }

        private string _status = "pending";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _vacancyTitle = "Загрузка...";
        public string VacancyTitle
        {
            get => _vacancyTitle;
            set => SetProperty(ref _vacancyTitle, value);
        }

        public string StatusColor => Status switch
        {
            "accepted" => "#4CAF50",  // Зелёный
            "rejected" => "#F44336",  // Красный
            "pending" => "#FFC107",   // Оранжевый
            _ => "#999999"
        };

        public string StatusText => Status switch
        {
            "accepted" => "Принята",
            "rejected" => "Отклонена",
            "pending" => "На рассмотрении",
            _ => Status
        };
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

        [ObservableProperty]
        private ObservableCollection<VacancyResponseViewModel> myResponses = new();

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
                    await Application.Current!.MainPage!.DisplayAlert(
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

                    // Загружаем мои отклики
                    await LoadMyResponses(token);
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Ошибка",
                        "Не удалось загрузить профиль",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load profile error: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert(
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

        private async Task LoadMyResponses(string token)
        {
            try
            {
                var responses = await _apiClient.GetAsJsonAsync<List<VacancyResponseItem>>(
                    "/responses/me",
                    token
                );

                MyResponses.Clear();

                if (responses != null && responses.Count > 0)
                {
                    foreach (var response in responses)
                    {
                        var responseViewModel = new VacancyResponseViewModel
                        {
                            VacancyId = response.VacancyId,
                            Id = response.Id,
                            Status = response.Status ?? "pending"
                        };

                        // Загружаем название вакансии
                        await LoadVacancyTitle(responseViewModel, token);

                        MyResponses.Add(responseViewModel);
                    }

                    Debug.WriteLine($"Loaded {MyResponses.Count} responses");
                }
                else
                {
                    Debug.WriteLine("No responses found");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load responses error: {ex.Message}");
            }
        }

        private async Task LoadVacancyTitle(VacancyResponseViewModel response, string token)
        {
            try
            {
                var vacancyData = await _apiClient.GetAsJsonAsync<VacancyTitleResponse>(
                    $"/vacancies/{response.VacancyId}",
                    token
                );

                if (vacancyData != null && !string.IsNullOrEmpty(vacancyData.Title))
                {
                    response.VacancyTitle = vacancyData.Title;
                    Debug.WriteLine($"Loaded title for vacancy {response.VacancyId}: {vacancyData.Title}");
                }
                else
                {
                    response.VacancyTitle = $"Вакансия #{response.VacancyId}";
                    Debug.WriteLine($"Title not found for vacancy {response.VacancyId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load vacancy title error for {response.VacancyId}: {ex.Message}");
                response.VacancyTitle = $"Вакансия #{response.VacancyId}";
            }
        }

        [RelayCommand]
        public async Task NavigateToVacancies()
        {
            await Shell.Current.GoToAsync("//VacancySearchPage");
        }

        [RelayCommand]
        public async Task NavigateToEvents()
        {
            await Shell.Current.GoToAsync("//EventSearchPage");
        }
    }
}