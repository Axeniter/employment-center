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
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();
            builder.Services.AddTransient<EmployerViewModel>();
            builder.Services.AddTransient<ApplicantViewModel>();
            builder.Services.AddTransient<EmployerCreateProfileViewModel>();
            builder.Services.AddTransient<ApplicantCreateProfileViewModel>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<EmployerPage>();
            builder.Services.AddTransient<ApplicantPage>();
            builder.Services.AddTransient<EmployerCreateProfilePage>();
            builder.Services.AddTransient<ApplicantCreateProfilePage>();

            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ApiClient>();

            return builder.Build();
        }
    }
}
