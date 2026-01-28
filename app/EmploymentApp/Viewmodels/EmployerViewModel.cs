using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;
using EmploymentApp.Models;
using EmploymentApp.Services;

namespace EmploymentApp.Viewmodels
{
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

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("employer_id")]
        public string EmployerId { get; set; }
    }

    public class VacancyResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

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

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("employer_id")]
        public string EmployerId { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
    }

    public class EmployerProfileResponse
    {
        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("contact")]
        public string Contact { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public partial class EmployerViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

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

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string companyName = "Загрузка...";

        [ObservableProperty]
        private string companyPhone = "";

        [ObservableProperty]
        private string companyDescription = "";

        public EmployerViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;

            EventCollection = new ObservableCollection<Event>();
            VacancyCollection = new ObservableCollection<Vacancy>();
            DisplayedEvents = new ObservableCollection<Event>();
            DisplayedVacancies = new ObservableCollection<Vacancy>();

            IsVacancySelected = false;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadAllData();
        }

        [RelayCommand]
        private async Task RefreshData()
        {
            await LoadAllData();
        }

        private async Task LoadAllData()
        {
            IsLoading = true;

            try
            {
                var token = await _authService.GetAccessTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("❌ No token found");
                    return;
                }

                var profileTask = LoadEmployerProfile(token);
                var eventsTask = LoadEvents(token);
                var vacanciesTask = LoadVacancies(token);

                await Task.WhenAll(profileTask, eventsTask, vacanciesTask);

                UpdateColors();
                UpdateDisplayedCollections();

                Debug.WriteLine($"✅ All loaded - Events: {EventCollection.Count}, Vacancies: {VacancyCollection.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadEmployerProfile(string token)
        {
            try
            {
                var profile = await _apiClient.GetAsJsonAsync<EmployerProfileResponse>(
                    "/profile/me",
                    token
                );

                if (profile != null)
                {
                    CompanyName = profile.CompanyName ?? "Компания";
                    CompanyPhone = profile.Contact ?? "";
                    CompanyDescription = profile.Description ?? "";
                    Debug.WriteLine("✅ Profile loaded");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Profile error: {ex.Message}");
            }
        }

        private async Task LoadEvents(string token)
        {
            try
            {
                Debug.WriteLine("📅 Loading events...");

                var events = await _apiClient.GetAsJsonAsync<List<EventResponse>>(
                    "/events/me",
                    token
                );

                Debug.WriteLine($"Raw events response count: {events?.Count}");

                EventCollection.Clear();

                if (events != null && events.Count > 0)
                {
                    foreach (var eventData in events)
                    {
                        Debug.WriteLine($"Creating event - Id: {eventData.Id}, Title: {eventData.Title}");

                        try
                        {
                            var eventItem = new Event(
                                eventData.Id,
                                eventData.Title,
                                eventData.Description,
                                eventData.Location,
                                eventData.IsRemote,
                                eventData.Date,
                                eventData.EmployerId,
                                eventData.IsActive
                            );

                            EventCollection.Add(eventItem);
                            Debug.WriteLine($"✅ Added event: {eventItem.Title}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"❌ Error creating event: {ex.Message}");
                        }
                    }
                    Debug.WriteLine($"✅ Events loaded: {EventCollection.Count}");
                }
                else
                {
                    Debug.WriteLine("⚠️ No events (null or empty)");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Events error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async Task LoadVacancies(string token)
        {
            try
            {
                Debug.WriteLine("💼 Loading vacancies...");

                var vacancies = await _apiClient.GetAsJsonAsync<List<VacancyResponse>>(
                    "/vacancies/me",
                    token
                );

                Debug.WriteLine($"Raw vacancies response count: {vacancies?.Count}");

                VacancyCollection.Clear();

                if (vacancies != null && vacancies.Count > 0)
                {
                    foreach (var vacancyData in vacancies)
                    {
                        Debug.WriteLine($"Creating vacancy - Id: {vacancyData.Id}, Title: {vacancyData.Title}");

                        try
                        {
                            var vacancy = new Vacancy(
                                vacancyData.Id,
                                vacancyData.Title,
                                vacancyData.Description,
                                vacancyData.SalaryFrom,
                                vacancyData.SalaryTo,
                                vacancyData.Location,
                                vacancyData.IsRemote,
                                vacancyData.EmployerId,
                                vacancyData.Tags ?? new List<string>(),
                                vacancyData.SalaryCurrency,
                                vacancyData.IsActive
                            );

                            VacancyCollection.Add(vacancy);
                            Debug.WriteLine($"✅ Added vacancy: {vacancy.Title}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"❌ Error creating vacancy: {ex.Message}");
                        }
                    }
                    Debug.WriteLine($"✅ Vacancies loaded: {VacancyCollection.Count}");
                }
                else
                {
                    Debug.WriteLine("⚠️ No vacancies (null or empty)");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Vacancies error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        partial void OnIsVacancySelectedChanged(bool value)
        {
            UpdateColors();
            UpdateDisplayedCollections();
            Debug.WriteLine($"View switched - Vacancies: {IsVacancySelected}");
        }

        private void UpdateColors()
        {
            if (IsVacancySelected)
            {
                EventColor = Color.FromArgb("#e0e0e0");
                VacancyColor = Color.FromArgb("#9dfca8");
            }
            else
            {
                EventColor = Color.FromArgb("#9dfca8");
                VacancyColor = Color.FromArgb("#e0e0e0");
            }
        }

        private void UpdateDisplayedCollections()
        {
            if (IsVacancySelected)
            {
                DisplayedEvents.Clear();
                DisplayedVacancies.Clear();
                foreach (var vacancy in VacancyCollection)
                {
                    DisplayedVacancies.Add(vacancy);
                }
                Debug.WriteLine($"Showing vacancies: {DisplayedVacancies.Count}");
            }
            else
            {
                DisplayedVacancies.Clear();
                DisplayedEvents.Clear();
                foreach (var evt in EventCollection)
                {
                    DisplayedEvents.Add(evt);
                }
                Debug.WriteLine($"Showing events: {DisplayedEvents.Count}");
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

        [RelayCommand]
        private void EventSelected(Event selectedEvent)
        {
            if (selectedEvent != null)
            {
                Debug.WriteLine($"Event selected: {selectedEvent.Title}");
            }
        }

        [RelayCommand]
        private void VacancySelected(Vacancy selectedVacancy)
        {
            if (selectedVacancy != null)
            {
                Debug.WriteLine($"Vacancy selected: {selectedVacancy.Title}");
            }
        }
    }
}