using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Models;
using EmploymentApp.Services;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EmploymentApp.Viewmodels
{
    public class EmployerProfileResponse
    {
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("contact")]
        public string Contact { get; set; }
    }

    public class VacancyResponse
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

    public class EventResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("is_remote")]
        public bool IsRemote { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("employer_id")]
        public string EmployerId { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

    }

    public partial class EmployerViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string companyName;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string contacts;

        [ObservableProperty]
        private bool isVacancySelected;

        [ObservableProperty]
        private ObservableCollection<Event> eventCollection;

        [ObservableProperty]
        private ObservableCollection<Vacancy> vacancyCollection;

        [ObservableProperty]
        private ObservableCollection<Event> displayedEvents;

        [ObservableProperty]
        private ObservableCollection<Vacancy> displayedVacancies;



        [ObservableProperty]
        private Color eventColor = Color.FromArgb("#9dfca8");

        [ObservableProperty]
        private Color vacancyColor = Color.FromArgb("#e0e0e0");

        public EmployerViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;

            UpdateColors();
            
            LoadProfile();
            LoadVacancies();
            LoadEvents();

            // Инициализация коллекции событий
            EventCollection = new ObservableCollection<Event>();

            // Инициализация коллекции вакансий
            VacancyCollection = new ObservableCollection<Vacancy>();

            // Инициализация отображаемых коллекций
            DisplayedEvents = new ObservableCollection<Event>(EventCollection);
            DisplayedVacancies = new ObservableCollection<Vacancy>();
        }

        private async Task LoadProfile()
        {
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

                    return;
                }

                var profileData = await _apiClient.GetAsJsonAsync<EmployerProfileResponse>(
                    "/profile/me",
                    token
                );

                if (profileData != null)
                {
                    CompanyName = profileData.CompanyName ?? "";
                    Contacts = profileData.Contact ?? "";
                    Description = profileData.Description ?? "";
                    
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
        }

        private async Task LoadVacancies()
        {
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
                    return;
                }

                var vacanciesResponse = await _apiClient.GetAsJsonAsync<List<VacancyResponse>>("/vacancies/me", token);

                if (vacanciesResponse != null && vacanciesResponse.Count > 0)
                {
                    DisplayedVacancies.Clear();

                    foreach (var vacancy in vacanciesResponse)
                    {
                        VacancyCollection.Add(new Vacancy(
                            vacancy.Id,
                            vacancy.Title,
                            vacancy.Description,
                            vacancy.SalaryFrom,
                            vacancy.SalaryTo,
                            vacancy.Location,
                            vacancy.IsRemote,
                            vacancy.EmployerId,
                            vacancy.Tags,
                            vacancy.SalaryCurrency,
                            vacancy.IsActive
                        ));
                    }

                    Debug.WriteLine($"Vacancies loaded successfully: {DisplayedVacancies.Count}");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        "Не удалось загрузить вакансии",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load vacancies error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    $"Ошибка загрузки вакансий: {ex.Message}",
                    "OK"
                );
            }
        }

        private async Task LoadEvents()
        {
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
                    return;
                }

                var eventsResponse = await _apiClient.GetAsJsonAsync<List<EventResponse>>("/events/me", token);

                if (eventsResponse != null && eventsResponse.Count > 0)
                {
                    DisplayedEvents.Clear();

                    foreach (var eventItem in eventsResponse)
                    {
                        EventCollection.Add(new Event(
                            eventItem.Id,
                            eventItem.Title,
                            eventItem.Description,
                            eventItem.Location,
                            eventItem.IsRemote,
                            eventItem.Date,
                            eventItem.EmployerId,
                            true
                        ));
                    }

                    Debug.WriteLine($"Events loaded successfully: {DisplayedEvents.Count}");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        "Не удалось загрузить события",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load events error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    $"Ошибка загрузки событий: {ex.Message}",
                    "OK"
                );
            }
        }

        partial void OnIsVacancySelectedChanged(bool value)
        {
            UpdateColors();
            UpdateDisplayedCollections();
        }

        private void UpdateColors()
        {
            EventColor = IsVacancySelected ? Color.FromArgb("#e0e0e0") : Color.FromArgb("#9dfca8");
            VacancyColor = IsVacancySelected ? Color.FromArgb("#9dfca8") : Color.FromArgb("#e0e0e0");
        }

        private void UpdateDisplayedCollections()
        {
            if (IsVacancySelected)
            {
                DisplayedEvents.Clear();
                DisplayedVacancies = new ObservableCollection<Vacancy>(VacancyCollection);
            }
            else
            {
                DisplayedVacancies.Clear();
                DisplayedEvents = new ObservableCollection<Event>(EventCollection);
            }
        }

        [RelayCommand]
        private void EventTapped()
        {
            IsVacancySelected = false;
        }

        [RelayCommand]
        private void VacancyTapped()
        {
            IsVacancySelected = true;
        }
    }
}
