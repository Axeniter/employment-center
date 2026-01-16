namespace EmploymentApp.Pages;

public partial class RegistrationPage : ContentPage
{
    private bool _isEmployerSelected;

    public RegistrationPage()
    {
        InitializeComponent();
        SetApplicantSelected(); // по умолчанию соискатель
    }

    private void ApplicantBorder_Tapped(object sender, TappedEventArgs e)
    {
        _isEmployerSelected = false;
        SetApplicantSelected();
    }

    private void EmployerBorder_Tapped(object sender, TappedEventArgs e)
    {
        _isEmployerSelected = true;
        SetEmployerSelected();
    }

    private void SetApplicantSelected()
    {
        ApplicantBorder.BackgroundColor = Color.FromArgb("#9dfca8");
        EmployerBorder.BackgroundColor = Color.FromArgb("#e0e0e0");
    }

    private void SetEmployerSelected()
    {
        ApplicantBorder.BackgroundColor = Color.FromArgb("#e0e0e0");
        EmployerBorder.BackgroundColor = Color.FromArgb("#9dfca8");
    }

    // В RegisterButton_Click добавь проверку паролей
    private void RegisterButton_Clicked(object sender, EventArgs e)
    {
        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            DisplayAlert("Ошибка", "Пароли не совпадают", "OK");
            return;
        }
        // TODO: регистрация
    }
}
