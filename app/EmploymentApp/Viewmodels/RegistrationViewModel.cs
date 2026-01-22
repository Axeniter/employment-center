using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace EmploymentApp.Viewmodels;

public partial class RegistrationViewModel : ObservableObject
{
    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string confirmPassword;

    [ObservableProperty]
    private bool isEmployerSelected;

    [ObservableProperty]
    private Color applicantColor = Color.FromArgb("#9dfca8");

    [ObservableProperty]
    private Color employerColor = Color.FromArgb("#e0e0e0");

    public RegistrationViewModel()
    {
        UpdateColors();  // Инициализация цветов
    }

    partial void OnIsEmployerSelectedChanged(bool value)
    {
        UpdateColors();
    }

    private void UpdateColors()
    {
        ApplicantColor = IsEmployerSelected ? Color.FromArgb("#e0e0e0") : Color.FromArgb("#9dfca8");
        EmployerColor = IsEmployerSelected ? Color.FromArgb("#9dfca8") : Color.FromArgb("#e0e0e0");
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
    private async Task LoginTapped()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }

    [RelayCommand]
    private async Task RegisterTapped()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите почту", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите пароль", "OK");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Пароли не совпадают", "OK");
            Password = "";
            ConfirmPassword = "";
            return;
        }

        if (Password.Length < 8)
        {
            await Application.Current.MainPage.DisplayAlert("Ошибка", "Длина пароля меньше 8 символов", "OK");
            Password = "";
            return;
        }

        await Application.Current.MainPage.DisplayAlert("Успех", "Аккаунт создан", "OK");
        // TODO: API регистрация
    }
}
