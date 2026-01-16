namespace EmploymentApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private void RegisterLabel_Tapped(object sender, TappedEventArgs e)
    {
        Shell.Current.GoToAsync("//RegistrationPage");
    }


}
