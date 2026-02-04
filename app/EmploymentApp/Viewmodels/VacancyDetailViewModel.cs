using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;


namespace EmploymentApp.Viewmodels
{
    
    public class VacancyDetailResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("salary_from")]
        public int SalaryFrom { get; set; }

        [JsonPropertyName("salary_to")]
        public int SalaryTo { get; set; }

        [JsonPropertyName("salary_currency")]
        public string SalaryCurrency { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("is_remote")]
        public bool IsRemote { get; set; }

        [JsonPropertyName("employer_id")]
        public string EmployerId { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
    }

    [QueryProperty(nameof(VacancyId), "id")]
    public partial class VacancyDetailViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private int vacancyId;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> tags = new();

        [ObservableProperty]
        private string tagsAsString = string.Empty;

        [ObservableProperty]
        private string salaryRange = string.Empty;

        [ObservableProperty]
        private string location = string.Empty;

        [ObservableProperty]
        private bool isRemote;

        [ObservableProperty]
        private string workType = "Не указано";

        [ObservableProperty]
        private string workLocation = string.Empty;

        [ObservableProperty]
        private string employerId = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string userRole = string.Empty;

        [ObservableProperty]
        private bool isApplicant;

        [ObservableProperty]
        private bool hasApplied;

        [ObservableProperty]
        private string applyButtonText = "ОТКЛИКНУТЬСЯ";

        // Информация о работодателе
        [ObservableProperty]
        private string companyName = string.Empty;

        [ObservableProperty]
        private string companyDescription = string.Empty;

        [ObservableProperty]
        private string companyContact = string.Empty;

        public VacancyDetailViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
        }

        partial void OnVacancyIdChanged(int value)
        {
            if (value > 0)
            {
                LoadVacancyDetailsCommand.Execute(null);
            }
        }

        [RelayCommand]
        public async Task LoadVacancyDetails()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                if (VacancyId <= 0)
                {
                    HasError = true;
                    ErrorMessage = "Некорректный ID вакансии";
                    return;
                }

                var token = await _authService.GetAccessTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    HasError = true;
                    ErrorMessage = "Не авторизованы";
                    return;
                }

                UserRole = await _authService.GetUserRoleAsync();
                IsApplicant = UserRole?.ToLower() == "applicant";

                var response = await _apiClient.GetAsJsonAsync<VacancyDetailResponse>($"/vacancies/{VacancyId}", token);

                if (response != null)
                {
                    Title = response.Title ?? string.Empty;
                    Description = response.Description ?? string.Empty;
                    SalaryRange = $"{response.SalaryFrom:N0} - {response.SalaryTo:N0} {response.SalaryCurrency}";
                    Location = response.Location ?? string.Empty;
                    IsRemote = response.IsRemote;
                    WorkLocation = IsRemote ? "Удаленно" : Location;
                    WorkType = IsRemote ? "Удаленная работа" : "Не удаленная";
                    EmployerId = response.EmployerId ?? string.Empty;

                    Tags.Clear();
                    if (response.Tags != null && response.Tags.Count > 0)
                    {
                        foreach (var tag in response.Tags)
                        {
                            Tags.Add(tag);
                        }
                        TagsAsString = string.Join(", ", response.Tags);
                    }
                    else
                    {
                        TagsAsString = "Не указаны";
                    }

                    Debug.WriteLine($"Loaded vacancy {VacancyId}: {Title}");

                    if (!string.IsNullOrEmpty(EmployerId))
                    {
                        await LoadEmployerProfile(EmployerId, token);
                    }
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Не удалось загрузить детали вакансии";
                    Debug.WriteLine($"Response is null for vacancy {VacancyId}");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Ошибка: {ex.Message}";
                Debug.WriteLine($"Error loading vacancy details: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadEmployerProfile(string employerId, string token)
        {
            try
            {
                var profileResponse = await _apiClient.GetAsJsonAsync<EmployerProfileResponse>($"/profile/{employerId}", token);

                if (profileResponse != null)
                {
                    CompanyName = profileResponse.CompanyName ?? "Компания не указана";
                    CompanyDescription = profileResponse.Description ?? "Описание отсутствует";
                    CompanyContact = profileResponse.Contact ?? "Контакт не указан";

                    Debug.WriteLine($"Loaded employer profile: {CompanyName}");
                }
                else
                {
                    CompanyName = "Не удалось загрузить информацию";
                    CompanyDescription = string.Empty;
                    CompanyContact = string.Empty;
                    Debug.WriteLine($"Employer profile response is null for {employerId}");
                }
            }
            catch (Exception ex)
            {
                CompanyName = "Ошибка загрузки";
                CompanyDescription = ex.Message;
                CompanyContact = string.Empty;
                Debug.WriteLine($"Error loading employer profile: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task ApplyToVacancy()
        {
            try
            {
                IsLoading = true;

                var token = await _authService.GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Ошибка",
                        "Вы не авторизованы",
                        "OK"
                    );
                    return;
                }

                var response = await _apiClient.PostAsync(
                    $"/responses/vacancy/{VacancyId}",
                    new { },
                    token
                );

                if (response.IsSuccessStatusCode)
                {
                    HasApplied = true;
                    ApplyButtonText = "ОТКЛИКНУЛИСЬ ✓";

                    await Application.Current!.MainPage!.DisplayAlert(
                        "Успех",
                        "Вы успешно откликнулись на вакансию!",
                        "OK"
                    );
                    Debug.WriteLine($"Successfully applied to vacancy {VacancyId}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Apply error: {response.StatusCode} - {errorContent}");

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest &&
                        errorContent.Contains("already responded"))
                    {
                        HasApplied = true;
                        ApplyButtonText = "ОТКЛИКНУЛИСЬ ✓";

                        await Application.Current!.MainPage!.DisplayAlert(
                            "Информация",
                            "Вы уже откликнулись на эту вакансию",
                            "OK"
                        );
                    }
                    else
                    {
                        await Application.Current!.MainPage!.DisplayAlert(
                            "Ошибка",
                            "Не удалось откликнуться на вакансию",
                            "OK"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying to vacancy: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    $"Ошибка: {ex.Message}",
                    "OK"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("//VacancySearchPage");
        }
    }
}