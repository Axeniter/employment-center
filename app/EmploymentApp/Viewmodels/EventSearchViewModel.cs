using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;


namespace EmploymentApp.Viewmodels
{
    public class EventItemViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public bool IsRemote { get; set; }

        public string WorkLocation => IsRemote ? "Удаленно" : Location;

        public DateTime Date { get; set; }

        public string FormattedDate => Date.ToString("dd.MM.yyyy");

        public string FormattedTime => Date.ToString("HH:mm");

        public string FormattedDateTime => $"{FormattedDate} в {FormattedTime}";

        public string EmployerId { get; set; }

        public bool IsActive { get; set; }
    }

    public partial class EventSearchViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private ObservableCollection<EventItemViewModel> events = new();

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
        private string dateFromString = string.Empty;

        [ObservableProperty]
        private string dateToString = string.Empty;

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int pageSize = 20;

        public EventSearchViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;

            LoadEvents();
        }

        [RelayCommand]
        public async Task LoadEvents()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var token = await _authService.GetAccessTokenAsync();

                var queryParams = BuildQueryParams();
                var url = $"/events/?{queryParams}";

                var response = await _apiClient.GetAsJsonAsync<List<EventResponse>>(url, token);

                if (response != null)
                {
                    var eventViewModels = response
                        .Where(e => e.IsActive)  
                        .Select(e => e.ToViewModel())
                        .ToList();

                    Events.Clear();
                    foreach (var @event in eventViewModels)
                    {
                        Events.Add(@event);
                    }

                    Debug.WriteLine($"Loaded {Events.Count} events");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "Не удалось загрузить события";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Ошибка: {ex.Message}";
                Debug.WriteLine($"Error loading events: {ex.Message}");
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

            if (!string.IsNullOrWhiteSpace(DateFromString))
            {
                if (DateTime.TryParse(DateFromString, out var dateFrom))
                {
                    parameters.Add($"date_from={dateFrom:yyyy-MM-ddTHH:mm:ss.fffZ}");
                }
            }

            if (!string.IsNullOrWhiteSpace(DateToString))
            {
                if (DateTime.TryParse(DateToString, out var dateTo))
                {
                    parameters.Add($"date_to={dateTo:yyyy-MM-ddTHH:mm:ss.fffZ}");
                }
            }

            return string.Join("&", parameters);
        }

        [RelayCommand]
        public async Task ApplyFilters()
        {
            CurrentPage = 1; 
            await LoadEvents();
        }

        [RelayCommand]
        public async Task ResetFilters()
        {
            SearchText = string.Empty;
            FilterLocation = string.Empty;
            FilterIsRemote = false;
            DateFromString = string.Empty;
            DateToString = string.Empty;
            CurrentPage = 1;

            await LoadEvents();
        }

        [RelayCommand]
        public async Task NavigateToVacancies()
        {
            await Shell.Current.GoToAsync("//VacancySearchPage");
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