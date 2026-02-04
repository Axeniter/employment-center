using EmploymentApp.Services;
using EmploymentApp.Viewmodels;
using System.Diagnostics;

namespace EmploymentApp.Pages;

public partial class VacancyDetailEmployerPage : ContentPage
{
    public VacancyDetailEmployerPage()
    {
        InitializeComponent();
        Debug.WriteLine("VacancyDetailEmployerPage constructor called");

        var viewModel = IPlatformApplication.Current!.Services
            .GetService<VacancyDetailEmployerViewModel>();

        if (viewModel != null)
        {
            BindingContext = viewModel;
            Debug.WriteLine("BindingContext set in constructor");
        }
        else
        {
            Debug.WriteLine("ViewModel is NULL!");
        }
    }

}
