using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class EventSearchPage : ContentPage
{
	public EventSearchPage(EventSearchViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}