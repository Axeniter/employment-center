using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System.Diagnostics;

namespace EmploymentApp.Viewmodels
{
    public partial class EmployerCreateProfileViewModel : ObservableObject
    {
        private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string companyName = "";

        [ObservableProperty]
        private string description = "";

        [ObservableProperty]
        private string contact = "";

        public EmployerCreateProfileViewModel(ApiClient apiClient, AuthService authService)
        {
            _apiClient = apiClient;
            _authService = authService;
        }

        [RelayCommand]
        private async Task SaveProfile()
        {

            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название компании", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите описание компании", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Contact))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите контактную информацию", "OK");
                return;
            }

            try
            {
                var profileData = new
                {
                    company_name = CompanyName,
                    description = Description,
                    contact = Contact
                };

                var token = await _authService.GetAccessTokenAsync();
                var response = await _apiClient.PostAsync("/profile/employer", profileData, token);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Успех", "Профиль сохранён", "OK");
                    await Shell.Current.GoToAsync("//EmployerPage");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить профиль", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Profile save error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            }
        }
    }
}
