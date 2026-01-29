using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Text.Json.Serialization;
using EmploymentApp.Services;

namespace EmploymentApp.Viewmodels
{
    public class EventCreateRequest
    {
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
    }

    public partial class EventCreateViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string description;

        [ObservableProperty]
        private string location;

        [ObservableProperty]
        private bool isRemote;

        [ObservableProperty]
        private string dateString;

        [ObservableProperty]
        private string timeString;

        [ObservableProperty]
        private bool isLoading;

        public EventCreateViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;

            // Устанавливаем значения по умолчанию
            DateString = DateTime.Now.ToString("yyyy-MM-dd");
            TimeString = DateTime.Now.ToString("HH:mm");
        }

        [RelayCommand]
        public async Task SaveEvent()
        {
            // Валидация: Название события
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название события", "OK");
                return;
            }

            // Валидация: Описание
            if (string.IsNullOrWhiteSpace(Description))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите описание события", "OK");
                return;
            }

            // Валидация: Локация
            if (string.IsNullOrWhiteSpace(Location))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите локацию события", "OK");
                return;
            }

            // Валидация: Дата
            if (string.IsNullOrWhiteSpace(DateString))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите дату события", "OK");
                return;
            }

            // Валидация: Время
            if (string.IsNullOrWhiteSpace(TimeString))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите время события", "OK");
                return;
            }

            // Парсим дату
            if (!DateTime.TryParse(DateString, out var eventDate))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Неверный формат даты (ГГГГ-ММ-ДД)", "OK");
                return;
            }

            // Проверяем формат времени (должен быть ЧЧ:ММ)
            if (!TimeString.Contains(":"))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Неверный формат времени (используйте ЧЧ:ММ, например 14:30)", "OK");
                return;
            }

            // Парсим время
            if (!TimeSpan.TryParse(TimeString, out var eventTime))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Неверный формат времени (ЧЧ:ММ)", "OK");
                return;
            }

            // Проверяем, что часы в диапазоне 0-23
            if (eventTime.TotalHours >= 24)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Часы должны быть от 00 до 23", "OK");
                return;
            }

            // Проверяем, что минуты в диапазоне 0-59
            if (eventTime.Minutes < 0 || eventTime.Minutes > 59)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Минуты должны быть от 00 до 59", "OK");
                return;
            }

            // Объединяем дату и время
            var eventDateTime = eventDate.Date.Add(eventTime);

            // Проверяем, что дата в будущем
            if (eventDateTime < DateTime.Now)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Дата события должна быть в будущем", "OK");
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

                var request = new EventCreateRequest
                {
                    Title = Title,
                    Description = Description,
                    Location = Location,
                    IsRemote = IsRemote,
                    Date = eventDateTime
                };

                var response = await _apiClient.PostAsync("/events/", request, token);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Успех",
                        "Событие успешно создано!",
                        "OK"
                    );

                    // Очищаем форму
                    ClearForm();

                    // Переходим на предыдущую страницу
                    await Shell.Current.GoToAsync("//EmployerPage");

                    Debug.WriteLine("Event created successfully");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error response: {response.StatusCode} - {errorContent}");
                    await Application.Current.MainPage.DisplayAlert("Ошибка", $"Не удалось создать событие: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating event: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task NavigateBack()
        {
            await Shell.Current.GoToAsync("//EmployerPage");
        }

        private void ClearForm()
        {
            Title = string.Empty;
            Description = string.Empty;
            Location = string.Empty;
            IsRemote = false;
            DateString = DateTime.Now.ToString("yyyy-MM-dd");
            TimeString = DateTime.Now.ToString("HH:mm");
        }
    }
}