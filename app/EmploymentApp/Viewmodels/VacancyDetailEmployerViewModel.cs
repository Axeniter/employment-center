using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace EmploymentApp.Viewmodels
{
    // ✅ Модель вакансии по реальному API
    public class VacancyDetailEmployerModel
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
        public string SalaryCurrency { get; set; } = "RUB";

        [JsonPropertyName("location")]
        public string WorkLocation { get; set; }

        [JsonPropertyName("is_remote")]
        public bool IsRemote { get; set; }

        [JsonPropertyName("employer_id")]
        public string EmployerId { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }
    }

    // Модель отклика
    public class VacancyResponseEmployerModel
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

    // ✅ Модель профиля с ВСЕ полями
    public class ApplicantProfileEmployerModel
    {
        [JsonPropertyName("profile_type")]
        public string ProfileType { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

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
        public List<string> Skills { get; set; } = new();
    }

    // ✅ ViewModel для отклика БЕЗ КОНВЕРТЕРОВ
    public class ResponseWithApplicantEmployerViewModel : ObservableObject
    {
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public string ApplicantId { get; set; }

        private string _status = "pending";
        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(IsPending));
                }
            }
        }

        private string _firstName = "";
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                {
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        private string _middleName = "";
        public string MiddleName
        {
            get => _middleName;
            set
            {
                if (SetProperty(ref _middleName, value))
                {
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        private string _lastName = "";
        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    OnPropertyChanged(nameof(FullName));
                }
            }
        }

        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (SetProperty(ref _phoneNumber, value))
                {
                    OnPropertyChanged(nameof(PhoneIsVisible));
                }
            }
        }

        private string _city = "";
        public string City
        {
            get => _city;
            set
            {
                if (SetProperty(ref _city, value))
                {
                    OnPropertyChanged(nameof(CityIsVisible));
                }
            }
        }

        private string _about = "";
        public string About
        {
            get => _about;
            set
            {
                if (SetProperty(ref _about, value))
                {
                    OnPropertyChanged(nameof(AboutIsVisible));
                }
            }
        }

        private string _skillsText = "";
        public string SkillsText
        {
            get => _skillsText;
            set
            {
                if (SetProperty(ref _skillsText, value))
                {
                    OnPropertyChanged(nameof(SkillsIsVisible));
                }
            }
        }

        // ✅ Вычисляемые свойства БЕЗ конвертеров
        public string StatusColor => Status switch
        {
            "accepted" => "#4CAF50",
            "rejected" => "#F44336",
            "pending" => "#FFC107",
            _ => "#999999"
        };

        public string StatusText => Status switch
        {
            "accepted" => "Принята",
            "rejected" => "Отклонена",
            "pending" => "На рассмотрении",
            _ => Status
        };

        public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();

        public bool IsPending => Status == "pending";

        public bool PhoneIsVisible => !string.IsNullOrWhiteSpace(PhoneNumber);

        public bool CityIsVisible => !string.IsNullOrWhiteSpace(City);

        public bool AboutIsVisible => !string.IsNullOrWhiteSpace(About);

        public bool SkillsIsVisible => !string.IsNullOrWhiteSpace(SkillsText);
    }

    public class UpdateResponseStatusRequest
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    // ✅ ГЛАВНАЯ ViewModel
    public partial class VacancyDetailEmployerViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private int vacancyId = 0;

        [ObservableProperty]
        private string vacancyTitle = "";

        [ObservableProperty]
        private string vacancyDescription = "";

        [ObservableProperty]
        private string salaryRange = "";

        [ObservableProperty]
        private string workLocation = "";

        [ObservableProperty]
        private string remoteText = "";

        [ObservableProperty]
        private bool showRemoteText = false;

        [ObservableProperty]
        private ObservableCollection<string> vacancyTags = new();

        [ObservableProperty]
        private bool hasTags = false;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private ObservableCollection<ResponseWithApplicantEmployerViewModel> responses = new();

        [ObservableProperty]
        private bool hasResponses = false;

        [ObservableProperty]
        private bool noResponses = true;

        public VacancyDetailEmployerViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
            Debug.WriteLine("VacancyDetailEmployerViewModel constructor");
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Debug.WriteLine("ApplyQueryAttributes called");

            if (query?.TryGetValue("id", out var idObj) == true)
            {
                string idString = idObj?.ToString() ?? "";
                Debug.WriteLine($"Got id from query: {idString}");

                if (int.TryParse(idString, out int id) && id > 0)
                {
                    VacancyId = id;
                    Debug.WriteLine($"VacancyId SET to: {VacancyId}");

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await LoadVacancyDetailsCommand.ExecuteAsync(null);
                    });
                }
            }
        }

        [RelayCommand]
        public async Task LoadVacancyDetails()
        {
            Debug.WriteLine($"LoadVacancyDetails called, VacancyId: {VacancyId}");

            if (VacancyId <= 0)
            {
                Debug.WriteLine("ERROR: VacancyId is invalid");
                return;
            }

            IsLoading = true;
            Responses.Clear();

            try
            {
                var token = await _authService.GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Debug.WriteLine("Token is empty");
                    return;
                }

                Debug.WriteLine($"Fetching vacancy {VacancyId}");

                var vacancy = await _apiClient.GetAsJsonAsync<VacancyDetailEmployerModel>(
                    $"/vacancies/{VacancyId}",
                    token
                );

                if (vacancy != null)
                {
                    Debug.WriteLine($"Vacancy loaded: {vacancy.Title}");

                    VacancyTitle = vacancy.Title ?? "";
                    VacancyDescription = vacancy.Description ?? "";
                    WorkLocation = vacancy.WorkLocation ?? "";

                    // Зарплата
                    if (vacancy.SalaryFrom > 0 && vacancy.SalaryTo > 0)
                    {
                        string currency = vacancy.SalaryCurrency ?? "RUB";
                        string currencySymbol = currency == "RUB" ? "₽" : currency;
                        SalaryRange = $"{currencySymbol}{vacancy.SalaryFrom:N0} - {currencySymbol}{vacancy.SalaryTo:N0}";
                    }

                    // Удалённая работа
                    ShowRemoteText = vacancy.IsRemote;
                    RemoteText = vacancy.IsRemote ? "Удалённо" : "";

                    // Теги
                    VacancyTags.Clear();
                    if (vacancy.Tags != null && vacancy.Tags.Count > 0)
                    {
                        foreach (var tag in vacancy.Tags)
                        {
                            VacancyTags.Add(tag);
                        }
                        HasTags = true;
                    }
                    else
                    {
                        HasTags = false;
                    }

                    Debug.WriteLine("Vacancy details loaded");
                }

                await LoadResponses(token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load vacancy error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadResponses(string token)
        {
            try
            {
                Debug.WriteLine($"Loading responses for vacancy {VacancyId}");

                var responseList = await _apiClient.GetAsJsonAsync<List<VacancyResponseEmployerModel>>(
                    $"/responses/vacancy/{VacancyId}",
                    token
                );

                Debug.WriteLine($"Raw responses: {responseList?.Count ?? 0}");

                Responses.Clear();

                if (responseList != null && responseList.Count > 0)
                {
                    foreach (var response in responseList)
                    {
                        var vm = new ResponseWithApplicantEmployerViewModel
                        {
                            Id = response.Id,
                            VacancyId = response.VacancyId,
                            ApplicantId = response.ApplicantId,
                            Status = response.Status ?? "pending"
                        };

                        await LoadApplicantProfile(vm, token);
                        Responses.Add(vm);
                    }

                    HasResponses = true;
                    NoResponses = false;
                    Debug.WriteLine($"Loaded {Responses.Count} responses");
                }
                else
                {
                    HasResponses = false;
                    NoResponses = true;
                    Debug.WriteLine("No responses");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load responses error: {ex.Message}");
                HasResponses = false;
                NoResponses = true;
            }
        }

        private async Task LoadApplicantProfile(ResponseWithApplicantEmployerViewModel response, string token)
        {
            try
            {
                var profile = await _apiClient.GetAsJsonAsync<ApplicantProfileEmployerModel>(
                    $"/profile/{response.ApplicantId}",
                    token
                );

                if (profile != null)
                {
                    response.FirstName = profile.FirstName ?? "";
                    response.MiddleName = profile.MiddleName ?? "";
                    response.LastName = profile.LastName ?? "";
                    response.PhoneNumber = profile.PhoneNumber ?? "";
                    response.City = profile.City ?? "";
                    response.About = profile.About ?? "";

                    if (profile.Skills != null && profile.Skills.Count > 0)
                    {
                        response.SkillsText = string.Join(", ", profile.Skills);
                    }

                    Debug.WriteLine($"Loaded profile: {response.FullName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load profile error: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task AcceptResponse(int responseId)
        {
            await UpdateResponseStatus(responseId, "accepted");
        }

        [RelayCommand]
        public async Task RejectResponse(int responseId)
        {
            await UpdateResponseStatus(responseId, "rejected");
        }

        private async Task UpdateResponseStatus(int responseId, string status)
        {
            try
            {
                var token = await _authService.GetAccessTokenAsync();
                var request = new UpdateResponseStatusRequest { Status = status };
                var response = await _apiClient.PutAsync($"/responses/{responseId}", request, token);

                if (response.IsSuccessStatusCode)
                {
                    var item = Responses.FirstOrDefault(r => r.Id == responseId);
                    if (item != null)
                    {
                        item.Status = status;
                    }

                    var text = status == "accepted" ? "принята" : "отклонена";
                    await Application.Current!.MainPage!.DisplayAlert("Успех", $"Заявка {text}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update status error: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("//EmployerPage");
        }
    }
}
