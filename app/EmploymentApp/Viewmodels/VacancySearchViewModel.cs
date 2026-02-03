using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

        // Параметры фильтрации
        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string filterLocation = string.Empty;

        [ObservableProperty]
        private bool filterIsRemote = false;

        [ObservableProperty]
        private string minSalaryText = string.Empty;

        [ObservableProperty]
        private string maxSalaryText = string.Empty;

        [ObservableProperty]
        private string salaryCurrency = "RUB";

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int pageSize = 20;

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

                var queryParams = BuildQueryParams();
                var url = $"/vacancies/?{queryParams}";

                var response = await _apiClient.GetAsJsonAsync<List<VacancyResponse>>(url, token);

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

        private string BuildQueryParams()
        {
            var parameters = new List<string>();

            parameters.Add($"page={CurrentPage}");
            parameters.Add($"limit={PageSize}");

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                parameters.Add($"search_text={Uri.EscapeDataString(SearchText)}");
            }

            if (!string.IsNullOrWhiteSpace(FilterLocation))
            {
                parameters.Add($"location={Uri.EscapeDataString(FilterLocation)}");
            }

            if (FilterIsRemote)
            {
                parameters.Add("is_remote=true");
            }

            if (!string.IsNullOrWhiteSpace(MinSalaryText) && int.TryParse(MinSalaryText, out var minSalary))
            {
                parameters.Add($"min_salary={minSalary}");
            }

            if (!string.IsNullOrWhiteSpace(MaxSalaryText) && int.TryParse(MaxSalaryText, out var maxSalary))
            {
                parameters.Add($"max_salary={maxSalary}");
            }

            if (!string.IsNullOrWhiteSpace(SalaryCurrency))
            {
                parameters.Add($"salary_currency={SalaryCurrency}");
            }

            return string.Join("&", parameters);
        }

        [RelayCommand]
        public async Task ApplyFilters()
        {
            CurrentPage = 1; 
            await LoadVacancies();
        }

        [RelayCommand]
        public async Task ResetFilters()
        {
            SearchText = string.Empty;
            FilterLocation = string.Empty;
            FilterIsRemote = false;
            MinSalaryText = string.Empty;
            MaxSalaryText = string.Empty;
            SalaryCurrency = "RUB";
            CurrentPage = 1;

            await LoadVacancies();
        }

        [RelayCommand]
        public async Task NavigateToEvents()
        {
            await Shell.Current.GoToAsync("//EventSearchPage");
        }

        [RelayCommand]
        public async Task NavigateToProfile()
        {
            if (await _authService.GetUserRoleAsync() == "applicant")
                await Shell.Current.GoToAsync("//ApplicantPage");

            else if (await _authService.GetUserRoleAsync() == "employer")
                await Shell.Current.GoToAsync("//EmployerPage");
        }

        [RelayCommand]
        public async Task SelectVacancy(int vacancyId)
        {
            Debug.WriteLine($"SelectVacancy called with vacancyId: {vacancyId}"); 

            await Shell.Current.GoToAsync($"///vacancydetail?id={vacancyId}");
        }
    }
}