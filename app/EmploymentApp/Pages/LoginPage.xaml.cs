namespace EmploymentApp.Pages;

public partial class LoginPage : ContentPage
{
    private bool _isEmployerSelected;

    public LoginPage()
    {
        InitializeComponent();
        SetApplicantSelected(); 
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

        LoginButton.BorderColor = Colors.Transparent; 
    }

    private void SetEmployerSelected()
    {
        ApplicantBorder.BackgroundColor = Color.FromArgb("#e0e0e0");
        EmployerBorder.BackgroundColor = Color.FromArgb("#9dfca8");

        LoginButton.BorderColor = Colors.Black; 
    }
}
