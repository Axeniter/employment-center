using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class EmployerPage : ContentPage
{
    private readonly EmployerViewModel _viewModel;

	public EmployerPage(EmployerViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
		BindingContext = _viewModel;
	}
}



