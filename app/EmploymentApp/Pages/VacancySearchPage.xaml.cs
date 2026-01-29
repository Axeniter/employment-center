using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class VacancySearchPage : ContentPage
{
	public VacancySearchPage(VacancySearchViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}