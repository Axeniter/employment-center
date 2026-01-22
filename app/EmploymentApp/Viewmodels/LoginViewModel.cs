using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmploymentApp.Viewmodels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        public LoginViewModel() { }

        [RelayCommand]
        private async Task NavigateToRegistration()
        {
            await Shell.Current.GoToAsync("//RegistrationPage");
        }
    }
}
