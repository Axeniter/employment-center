using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Text.Json.Serialization;
using EmploymentApp.Services;

namespace EmploymentApp.Viewmodels
{
    public class VacancyCreateRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

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
    }

    public partial class VacancyCreateViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string tagsText;

        [ObservableProperty]
        private string salaryFromString;

        [ObservableProperty]
        private string salaryToString;

        [ObservableProperty]
        private string salaryCurrency = "RUB";

        [ObservableProperty]
        private string location;

        [ObservableProperty]
        private bool isRemote;

        [ObservableProperty]
        private bool isLoading;

        public VacancyCreateViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
        }

        [RelayCommand]
        public async Task SaveVacancy()
        {
            // Валидация: Название вакансии
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название вакансии", "OK");
                return;
            }

            // Валидация: Описание
            if (string.IsNullOrWhiteSpace(Description))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите описание вакансии", "OK");
                return;
            }

            // Валидация: Локация
            if (string.IsNullOrWhiteSpace(Location))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите локацию", "OK");
                return;
            }

            // Валидация: Зарплата от (наличие)
            if (string.IsNullOrWhiteSpace(SalaryFromString))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите зарплату 'от'", "OK");
                return;
            }

            // Валидация: Зарплата до (наличие)
            if (string.IsNullOrWhiteSpace(SalaryToString))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите зарплату 'до'", "OK");
                return;
            }

            // Валидация: Зарплата от (число)
            if (!int.TryParse(SalaryFromString, out int salaryFrom))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Зарплата 'от' должна быть числом", "OK");
                return;
            }

            // Валидация: Зарплата до (число)
            if (!int.TryParse(SalaryToString, out int salaryTo))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Зарплата 'до' должна быть числом", "OK");
                return;
            }

            // Валидация: Зарплата от <= до
            if (salaryFrom > salaryTo)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Зарплата 'от' не может быть больше зарплаты 'до'", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                var token = await _authService.GetAccessTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Вы не авторизованы", "OK");
                    return;
                }

                // Разбираем теги по запятой
                var tags = string.IsNullOrEmpty(TagsText)
                    ? new List<string>()
                    : TagsText.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();

                var request = new VacancyCreateRequest
                {
                    Title = Title,
                    Description = Description,
                    Tags = tags,
                    SalaryFrom = salaryFrom,
                    SalaryTo = salaryTo,
                    SalaryCurrency = SalaryCurrency,
                    Location = Location,
                    IsRemote = IsRemote
                };

                var response = await _apiClient.PostAsync("/vacancies/", request, token);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Успех",
                        "Вакансия успешно создана!",
                        "OK"
                    );

                    // Очищаем форму
                    ClearForm();

                    // Переходим на предыдущую страницу
                    await Shell.Current.GoToAsync("//EmployerPage");

                    Debug.WriteLine("Vacancy created successfully");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error response: {response.StatusCode} - {errorContent}");
                    await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось создать вакансию: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating vacancy: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearForm()
        {
            Title = string.Empty;
            Description = string.Empty;
            TagsText = string.Empty;
            SalaryFromString = string.Empty;
            SalaryToString = string.Empty;
            SalaryCurrency = "RUB";
            Location = string.Empty;
            IsRemote = false;
        }

        [RelayCommand]
        private async Task NavigateBack()
        {
            await Shell.Current.GoToAsync("//EmployerPage");
        }
    }
}