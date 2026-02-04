using EmploymentApp.Viewmodels;
using EmploymentApp.Pages;
using EmploymentApp.Services;
using Microsoft.Extensions.Logging;

namespace EmploymentApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Logging.AddDebug();

            // ✅ SINGLETON services FIRST (создаются один раз)
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ApiClient>();

            // ✅ ViewModels - используем SINGLETON для detail страниц!
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();
            builder.Services.AddTransient<EmployerViewModel>();
            builder.Services.AddTransient<ApplicantViewModel>();
            builder.Services.AddTransient<EmployerCreateProfileViewModel>();
            builder.Services.AddTransient<ApplicantCreateProfileViewModel>();
            builder.Services.AddTransient<EventCreateViewModel>();
            builder.Services.AddTransient<VacancyCreateViewModel>();
            builder.Services.AddTransient<VacancySearchViewModel>();
            builder.Services.AddTransient<EventSearchViewModel>();
            builder.Services.AddTransient<VacancyDetailViewModel>();

            // ✅ SINGLETON для VacancyDetailEmployerViewModel!
            builder.Services.AddSingleton<VacancyDetailEmployerViewModel>();

            // ✅ Pages - используем SINGLETON для detail страниц!
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<EmployerPage>();
            builder.Services.AddTransient<ApplicantPage>();
            builder.Services.AddTransient<EmployerCreateProfilePage>();
            builder.Services.AddTransient<ApplicantCreateProfilePage>();
            builder.Services.AddTransient<EventCreatePage>();
            builder.Services.AddTransient<VacancyCreatePage>();
            builder.Services.AddTransient<VacancySearchPage>();
            builder.Services.AddTransient<EventSearchPage>();
            builder.Services.AddTransient<VacancyDetailPage>();

            // ✅ SINGLETON для VacancyDetailEmployerPage!
            builder.Services.AddSingleton<VacancyDetailEmployerPage>();

            return builder.Build();
        }
    }
}
