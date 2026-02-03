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

        var viewModel = IPlatformApplication.Current!.Services.GetService<VacancyDetailEmployerViewModel>();
        BindingContext = viewModel;

        if (viewModel != null)
        {
            // Попробуй получить ID из query параметров
            var route = Shell.Current.CurrentState.Location.ToString();
            Debug.WriteLine($"Current route: {route}");

            // Провери есть ли в маршруте вообще query параметры
            if (route.Contains("?"))
            {
                var idParam = GetQueryParam(route, "id");
                Debug.WriteLine($"ID param: {idParam}");

                if (!string.IsNullOrEmpty(idParam) && int.TryParse(idParam, out int vacancyId))
                {
                    viewModel.VacancyId = vacancyId;
                    await viewModel.LoadVacancyDetailsCommand.ExecuteAsync(null);
                }
            }
            else
            {
                Debug.WriteLine("No query parameters in route!");
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