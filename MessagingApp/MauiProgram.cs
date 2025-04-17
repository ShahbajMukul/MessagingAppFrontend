using Blazored.LocalStorage;
using MessagingApp.Shared.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
#if ANDROID
using Android.Net;
using Xamarin.Android.Net;
#endif
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

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();


            // BackendAPI
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new System.Uri("https://localhost:5500/api/") });

//            builder.Services.AddScoped(sp =>
//            {
//#if ANDROID
//                    var handler = new AndroidMessageHandler();
//                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
//                    {
//#if DEBUG
//                        // Only allow this in development
//                        return true;
//#else
//                        return errors == System.Net.Security.SslPolicyErrors.None;
//#endif
//                    };
//#elif DEBUG
//                var handler = new HttpClientHandler();
//#else
//                    var handler = new HttpClientHandler();
//#endif

//                return new HttpClient(handler)
//                {
//                    BaseAddress = new System.Uri("https://10.0.0.23:5500/api/")  // Your dev machine's IP
//                };
//            });
            builder.Services.AddScoped<MessagingService>();
            builder.Services.AddScoped<CryptographyService>();



#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
