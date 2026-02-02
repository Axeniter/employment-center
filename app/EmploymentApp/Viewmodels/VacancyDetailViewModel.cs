using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;


namespace EmploymentApp.Viewmodels
{
    public class EmployerInfoViewModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string CompanyName { get; set; }

        public string PhoneNumber { get; set; }
    }

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
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private ObservableCollection<string> tags = new();

        [ObservableProperty]
        private string salaryRange;

        [ObservableProperty]
        private string location;

        [ObservableProperty]
        private bool isRemote;

        [ObservableProperty]
        private string workLocation;

        [ObservableProperty]
        private string employerId;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string userRole; // "applicant" или "employer"

        [ObservableProperty]
        private bool isApplicant;

        [ObservableProperty]
        private bool hasApplied;

        [ObservableProperty]
        private string applyButtonText = "ОТКЛИКНУТЬСЯ";

        public VacancyDetailViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;

            LoadVacancyDetails();
        }

        // Автоматически вызывается когда меняется VacancyId
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

                var token = await _authService.GetAccessTokenAsync();

                // Получаем роль пользователя
                UserRole = await _authService.GetUserRoleAsync();
                IsApplicant = UserRole?.ToLower() == "applicant";

                var response = await _apiClient.GetAsJsonAsync<VacancyDetailResponse>($"/vacancies/{VacancyId}", token);

                if (response != null)
                {
                    Title = response.Title;
                    Description = response.Description;
                    SalaryRange = $"{response.SalaryFrom:N0} - {response.SalaryTo:N0} {response.SalaryCurrency}";
                    Location = response.Location;
                    IsRemote = response.IsRemote;
                    WorkLocation = IsRemote ? "Удаленно" : Location;
                    EmployerId = response.EmployerId;

                    Tags.Clear();
                    foreach (var tag in response.Tags)
                    {
                        Tags.Add(tag);
                    }

                    Debug.WriteLine("Vacancy details loaded successfully");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Не удалось загрузить детали вакансии";
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

        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}