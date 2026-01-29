using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class VacancyCreatePage : ContentPage
{
	public VacancyCreatePage(VacancyCreateViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		
	}
}