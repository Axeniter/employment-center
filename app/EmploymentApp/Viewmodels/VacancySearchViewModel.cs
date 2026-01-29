using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace EmploymentApp.Viewmodels
{
    public class VacancyItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<string> Tags { get; set; } = new();

        public int SalaryFrom { get; set; }

        public int SalaryTo { get; set; }

        public string SalaryCurrency { get; set; }

        public string SalaryRange => $"{SalaryFrom:N0} - {SalaryTo:N0} {SalaryCurrency}";

        public string Location { get; set; }

        public bool IsRemote { get; set; }

        public string WorkLocation => IsRemote ? "Удаленно" : Location;

        public DateTime CreatedAt { get; set; }

        public VacancyItemViewModel ToViewModel()
        {
            return new VacancyItemViewModel
            {
                Id = Id,
                Title = Title,
                Description = Description,
                Tags = Tags ?? new(),
                SalaryFrom = SalaryFrom,
                SalaryTo = SalaryTo,
                SalaryCurrency = SalaryCurrency,
                Location = Location,
                IsRemote = IsRemote,
                CreatedAt = CreatedAt
            };
        }
    }

    public partial class VacancySearchViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private ObservableCollection<VacancyItemViewModel> vacancies = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string searchQuery;

        public VacancySearchViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;

            LoadVacancies();
        }

        [RelayCommand]
        public async Task LoadVacancies()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var token = await _authService.GetAccessTokenAsync();

                var response = await _apiClient.GetAsJsonAsync<List<VacancyResponse>>("/vacancies/", token);

                if (response != null)
                {
                    var vacancyViewModels = response
                        .Select(v => v.ToViewModel())
                        .ToList();

                    Vacancies.Clear();

                    foreach (var vacancy in vacancyViewModels)
                    {
                        Vacancies.Add(vacancy);
                    }

                    Debug.WriteLine($"Loaded {Vacancies.Count} vacancies");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Не удалось загрузить вакансии";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Ошибка: {ex.Message}";
                Debug.WriteLine($"Error loading vacancies: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task NavigateToEvents()
        {
            await Shell.Current.GoToAsync("events");
        }

        [RelayCommand]
        public async Task NavigateToChat()
        {
            await Shell.Current.GoToAsync("chat");
        }

        [RelayCommand]
        public async Task NavigateToProfile()
        {
            if (await _authService.GetUserRoleAsync() == "applicant")
                await Shell.Current.GoToAsync("//ApplicantPage");

            else if (await _authService.GetUserRoleAsync() == "employer")
                await Shell.Current.GoToAsync("//EmployerPage");
        }
    }
}