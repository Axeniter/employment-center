using EmploymentApp.Services;
using EmploymentApp.Viewmodels;
using System.Diagnostics;

namespace EmploymentApp.Pages;

public partial class VacancyDetailEmployerPage : ContentPage
{
    public VacancyDetailEmployerPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Получаем ViewModel из Services напрямую
        var viewModel = IPlatformApplication.Current!.Services.GetService<VacancyDetailEmployerViewModel>();
        BindingContext = viewModel;

        if (viewModel != null)
        {
            // Получаем ID из query параметров
            var route = Shell.Current.CurrentState.Location.ToString();
            Debug.WriteLine($"Current route: {route}");

            if (route.Contains("vacancydetailemployer") && int.TryParse(GetQueryParam(route, "id"), out int vacancyId))
            {
                viewModel.VacancyId = vacancyId;

                Debug.WriteLine($"Viewmodel takes vacancyId");

                await viewModel.LoadVacancyDetailsCommand.ExecuteAsync(null);
            }
        }
    }

    private string GetQueryParam(string route, string paramName)
    {
        var queryIndex = route.IndexOf('?');
        if (queryIndex < 0) return null;

        var queryString = route.Substring(queryIndex + 1);
        var pairs = queryString.Split('&');

        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2 && keyValue[0] == paramName)
            {
                return Uri.UnescapeDataString(keyValue[1]);
            }
        }

        return null;
    }
}