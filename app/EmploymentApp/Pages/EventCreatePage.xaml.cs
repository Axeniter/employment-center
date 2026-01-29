using EmploymentApp.Viewmodels;

namespace EmploymentApp.Pages;

public partial class EventCreatePage : ContentPage
{
	public EventCreatePage(EventCreateViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}