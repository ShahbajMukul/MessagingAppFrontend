using Blazored.LocalStorage;
using MessagingApp.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<MessagingService>();
builder.Services.AddSingleton<CryptographyService>();
// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://10.0.0.23:5500/api/") });
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5500/api/") });


// Add device-specific services used by the MessagingApp.Shared project
// builder.Services.AddSingleton<IFormFactor, FormFactor>();

await builder.Build().RunAsync();
