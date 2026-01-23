using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploymentApp.Viewmodels
{
    public partial class LoginViewModel : ObservableObject
    {

        private readonly AuthService _authService;

        [ObservableProperty]
        private string email; 

        [ObservableProperty]
        private string password;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task LoginTapped()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Заполните все поля",
                    "OK"
                );
                return;
            }

            try
            {
                var loginResult = await _authService.LoginAsync(Email, Password);

                if (loginResult.Success)
                {
                    var userId = loginResult.UserId;
                    var role = loginResult.Role;

                    Debug.WriteLine($"Успешный вход!");
                    Debug.WriteLine($"User ID: {userId}");
                    Debug.WriteLine($"Role: {role}");

                    if (role == "applicant")
                    {
                        await Shell.Current.GoToAsync("//ApplicantPage");
                    }
                    else if (role == "employer")
                    {
                        await Shell.Current.GoToAsync("//EmployerPage");
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка входа",
                        "Неправильный email или пароль",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    $"Ошибка подключения: {ex.Message}",
                    "OK"
                );
            }
        }

        [RelayCommand]
        private async Task NavigateToRegistration()
        {
            await Shell.Current.GoToAsync("//RegistrationPage");
        }
    }
}
