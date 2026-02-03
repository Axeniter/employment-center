using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace EmploymentApp.Viewmodels
{
    // Модель вакансии
    public class VacancyDetailEmployerModel
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

        [JsonPropertyName("work_location")]
        public string WorkLocation { get; set; }

        [JsonPropertyName("employment_type")]
        public string EmploymentType { get; set; }

        [JsonPropertyName("requirements")]
        public List<string> Requirements { get; set; }

        [JsonPropertyName("employer_id")]
        public string EmployerId { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }
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

    // Модель профиля applicant
    public class ApplicantProfileEmployerModel
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonPropertyName("about")]
        public string About { get; set; }

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; }
    }

    // ViewModel для отклика с загруженными данными
    public class ResponseWithApplicantEmployerViewModel : ObservableObject
    {
        public int Id { get; set; }
        public int VacancyId { get; set; }
        public string ApplicantId { get; set; }

        private string _status = "pending";
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private string _firstName = "";
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = "";
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        private string _about = "";
        public string About
        {
            get => _about;
            set => SetProperty(ref _about, value);
        }

        private string _skillsText = "";
        public string SkillsText
        {
            get => _skillsText;
            set => SetProperty(ref _skillsText, value);
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

        public string FullName => $"{FirstName} {LastName}";
    }

    // Модель для PUT запроса
    public class UpdateResponseStatusRequest
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public partial class VacancyDetailEmployerViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        public int VacancyId { get; set; }

        [ObservableProperty]
        private string vacancyTitle = "";

        [ObservableProperty]
        private string vacancyDescription = "";

        [ObservableProperty]
        private string salaryRange = "";

        [ObservableProperty]
        private string workLocation = "";

        [ObservableProperty]
        private string employmentType = "";

        [ObservableProperty]
        private string requirementsText = "";

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private ObservableCollection<ResponseWithApplicantEmployerViewModel> responses = new();

        [ObservableProperty]
        private bool hasResponses = false;

        public VacancyDetailEmployerViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;

        }

        [RelayCommand]
        public async Task LoadVacancyDetails()
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
                    return;
                }

                // Загружаем детали вакансии
                var vacancy = await _apiClient.GetAsJsonAsync<VacancyDetailEmployerModel>(
                    $"/vacancies/{VacancyId}",
                    token
                );

                Debug.WriteLine($"vacancy loaded!");

                if (vacancy != null)
                {
                    VacancyTitle = vacancy.Title ?? "";
                    VacancyDescription = vacancy.Description ?? "";
                    WorkLocation = vacancy.WorkLocation ?? "";
                    EmploymentType = vacancy.EmploymentType ?? "";

                    // Форматируем зарплату
                    if (vacancy.SalaryFrom > 0 && vacancy.SalaryTo > 0)
                    {
                        SalaryRange = $"₽{vacancy.SalaryFrom:N0} - ₽{vacancy.SalaryTo:N0}";
                    }

                    // Требования
                    if (vacancy.Requirements != null && vacancy.Requirements.Count > 0)
                    {
                        RequirementsText = string.Join(", ", vacancy.Requirements);
                    }

                    Debug.WriteLine("Vacancy details loaded successfully");

                    // Загружаем отклики
                    await LoadResponses(token);
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Ошибка",
                        "Не удалось загрузить вакансию",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load vacancy details error: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    $"Ошибка загрузки: {ex.Message}",
                    "OK"
                );
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
                var responseList = await _apiClient.GetAsJsonAsync<List<VacancyResponseEmployerModel>>(
                    $"/responses/vacancy/{VacancyId}",
                    token
                );

                Responses.Clear();

                if (responseList != null && responseList.Count > 0)
                {
                    foreach (var response in responseList)
                    {
                        var responseViewModel = new ResponseWithApplicantEmployerViewModel
                        {
                            Id = response.Id,
                            VacancyId = response.VacancyId,
                            ApplicantId = response.ApplicantId,
                            Status = response.Status ?? "pending"
                        };

                        // Загружаем профиль applicant
                        await LoadApplicantProfile(responseViewModel, token);

                        Responses.Add(responseViewModel);
                    }

                    HasResponses = true;
                    Debug.WriteLine($"Loaded {Responses.Count} responses");
                }
                else
                {
                    HasResponses = false;
                    Debug.WriteLine("No responses found");
                }
            }
            catch (Exception ex)
            {
                HasResponses = false;
                Debug.WriteLine($"Load responses error: {ex.Message}");
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
                    response.LastName = profile.LastName ?? "";
                    response.PhoneNumber = profile.PhoneNumber ?? "";
                    response.About = profile.About ?? "";

                    if (profile.Skills != null && profile.Skills.Count > 0)
                    {
                        response.SkillsText = string.Join(", ", profile.Skills);
                    }

                    Debug.WriteLine($"Loaded profile for {response.ApplicantId}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Load applicant profile error for {response.ApplicantId}: {ex.Message}");
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

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Ошибка",
                        "Токен не найден",
                        "OK"
                    );
                    return;
                }

                var request = new UpdateResponseStatusRequest { Status = status };

                var response = await _apiClient.PutAsync(
                    $"/responses/{responseId}",
                    request,
                    token
                );

                if (response.IsSuccessStatusCode)
                {
                    // Обновляем статус в коллекции
                    var responseItem = Responses.FirstOrDefault(r => r.Id == responseId);
                    if (responseItem != null)
                    {
                        responseItem.Status = status;
                    }

                    var statusText = status == "accepted" ? "принята" : "отклонена";
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Успех",
                        $"Заявка {statusText}",
                        "OK"
                    );

                    Debug.WriteLine($"Response {responseId} status updated to {status}");
                }
                else
                {
                    await Application.Current!.MainPage!.DisplayAlert(
                        "Ошибка",
                        "Не удалось обновить статус",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Update response status error: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert(
                    "Ошибка",
                    $"Ошибка: {ex.Message}",
                    "OK"
                );
            }
        }

        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}