using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace EmploymentApp.Viewmodels
{
    public partial class RegistrationViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string email = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private string confirmPassword = "";

        [ObservableProperty]
        private bool isEmployerSelected = false;

        [ObservableProperty]
        private Color applicantColor = Color.FromArgb("#9dfca8");

        [ObservableProperty]
        private Color employerColor = Color.FromArgb("#e0e0e0");

        public RegistrationViewModel(AuthService authService)
        {
            _authService = authService;
            UpdateColors();
        }

        partial void OnIsEmployerSelectedChanged(bool value)
        {
            UpdateColors();
        }

        private void UpdateColors()
        {
            ApplicantColor = IsEmployerSelected
                ? Color.FromArgb("#e0e0e0")
                : Color.FromArgb("#9dfca8");

            EmployerColor = IsEmployerSelected
                ? Color.FromArgb("#9dfca8")
                : Color.FromArgb("#e0e0e0");
        }

        [RelayCommand]
        private void ApplicantTapped()
        {
            IsEmployerSelected = false;
        }

        [RelayCommand]
        private void EmployerTapped()
        {
            IsEmployerSelected = true;
        }

        [RelayCommand]
        private async Task RegisterTapped()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Введите email",
                    "OK"
                );
                return;
            }

            if (!Email.Contains("@") || !Email.Contains("."))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Введите корректный email",
                    "OK"
                );
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Введите пароль",
                    "OK"
                );
                return;
            }

            if (Password.Length < 8)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Пароль должен быть минимум 8 символов",
                    "OK"
                );
                Password = "";
                ConfirmPassword = "";
                return;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Подтвердите пароль",
                    "OK"
                );
                return;
            }

            if (Password != ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Пароли не совпадают",
                    "OK"
                );
                Password = "";
                ConfirmPassword = "";
                return;
            }

            var role = IsEmployerSelected ? "employer" : "applicant";

            try
            {
                var (success, userId) = await _authService.RegisterAsync(
                    Email,
                    Password,
                    role
                );

                if (success)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Успех",
                        "Аккаунт создан успешно!",
                        "OK"
                    );

                    Email = "";
                    Password = "";
                    ConfirmPassword = "";
                    IsEmployerSelected = false;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка регистрации",
                        "Не удалось создать аккаунт. Возможно, email уже зарегистрирован.",
                        "OK"
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Registration error: {ex.Message}");

                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    $"Ошибка подключения: {ex.Message}",
                    "OK"
                );
            }
        }

        [RelayCommand]
        private async Task NavigateToLogin()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}