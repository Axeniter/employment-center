using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class EmployerPage : ContentPage
{
	public EmployerPage(EmployerViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}