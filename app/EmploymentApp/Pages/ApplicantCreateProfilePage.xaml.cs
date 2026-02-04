using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class ApplicantCreateProfilePage : ContentPage
{
	public ApplicantCreateProfilePage(ApplicantCreateProfileViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}