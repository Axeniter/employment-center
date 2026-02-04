using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;


public partial class EmployerCreateProfilePage : ContentPage
{
	public EmployerCreateProfilePage(EmployerCreateProfileViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}