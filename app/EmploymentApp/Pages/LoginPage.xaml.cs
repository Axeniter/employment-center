using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

}
