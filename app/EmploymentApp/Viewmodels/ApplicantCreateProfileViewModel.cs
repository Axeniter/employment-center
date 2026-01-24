using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EmploymentApp.Services;

namespace EmploymentApp.Viewmodels
{
    public partial class ApplicantCreateProfileViewModel : ObservableObject
    {
        //private readonly ApiClient _apiClient;
        private readonly AuthService _authService;

        [ObservableProperty]
        private string firstName = "";

        [ObservableProperty]
        private string lastName = "";

        [ObservableProperty]
        private string middleName = "";

        [ObservableProperty]
        private string phoneNumber = "";

        [ObservableProperty]
        private string birthDateString = "";

        [ObservableProperty]
        private string city = "";

        [ObservableProperty]
        private string about = "";

        [ObservableProperty]
        private string skillsText = "";

        public ApplicantCreateProfileViewModel(/*ApiClient apiClient,*/ AuthService authService)
        {
            //_apiClient = apiClient;
            _authService = authService;
        }

        [RelayCommand]
        private async Task SaveProfile()
        {
            await Application.Current.MainPage.DisplayAlert("Успех!", "Профиль сохранен", "OK");

            //    if (string.IsNullOrWhiteSpace(FirstName))
            //    {
            //        await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите имя", "OK");
            //        return;
            //    }

            //    if (string.IsNullOrWhiteSpace(LastName))
            //    {
            //        await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите фамилию", "OK");
            //        return;
            //    }

            //    if (string.IsNullOrWhiteSpace(PhoneNumber))
            //    {
            //        await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите номер телефона", "OK");
            //        return;
            //    }

            //    if (!DateTime.TryParse(BirthDateString, out var birthDate))
            //    {
            //        await Application.Current.MainPage.DisplayAlert("Ошибка", "Неверная дата (ГГГГ-ММ-ДД)", "OK");
            //        return;
            //    }

            //    if (string.IsNullOrWhiteSpace(City))
            //    {
            //        await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите город", "OK");
            //        return;
            //    }

            //    try
            //    {
            //        var skills = string.IsNullOrWhiteSpace(SkillsText)
            //            ? new List<string>()
            //            : SkillsText.Split(',')
            //                .Select(s => s.Trim())
            //                .Where(s => !string.IsNullOrEmpty(s))
            //                .ToList();

            //        var profileData = new
            //        {
            //            first_name = FirstName,
            //            last_name = LastName,
            //            middle_name = MiddleName,
            //            phone_number = PhoneNumber,
            //            birth_date = birthDate.ToString("yyyy-MM-dd"),
            //            city = City,
            //            about = About,
            //            skills = skills
            //        };

            //        var token = await _authService.GetAccessTokenAsync();
            //        var response = await _apiClient.PostAsync("/applicant/profile", profileData, token);

            //        if (response.IsSuccessStatusCode)
            //        {
            //            await Application.Current.MainPage.DisplayAlert("Успех", "Профиль сохранён", "OK");
            //            await Shell.Current.GoToAsync("//ApplicantPage");
            //        }
            //        else
            //        {
            //            await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось сохранить профиль", "OK");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine($"Profile save error: {ex.Message}");
            //        await Application.Current.MainPage.DisplayAlert("Ошибка", $"Ошибка: {ex.Message}", "OK");
            //    }
        }
    }
}
