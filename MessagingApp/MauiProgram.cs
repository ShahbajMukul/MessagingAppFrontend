using Blazored.LocalStorage;
using MessagingApp.Shared.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace MessagingApp
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
                });

            // Add device-specific services used by the MessagingApp.Shared project
            // builder.Services.AddSingleton<IFormFactor, FormFactor>();

            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();


            // BackendAPI
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000/api/") });
            builder.Services.AddScoped<MessagingService>();
            builder.Services.AddSingleton<CryptographyService>();


            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
