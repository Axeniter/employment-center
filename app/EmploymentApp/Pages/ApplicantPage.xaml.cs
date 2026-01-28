using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class ApplicantPage : ContentPage
{
    private readonly ApplicantViewModel _viewModel;

    public ApplicantPage(ApplicantViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}