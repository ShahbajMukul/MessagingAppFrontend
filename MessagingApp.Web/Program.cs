using Blazored.LocalStorage;
using MessagingApp.Shared.Services;
using MessagingApp.Web.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add device-specific services used by the MessagingApp.Shared project
// builder.Services.AddSingleton<IFormFactor, FormFactor>();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<MessagingService>();
builder.Services.AddScoped<CryptographyService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5500/api/") });

// builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://10.0.0.23:5500/api/") });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(
        typeof(MessagingApp.Shared._Imports).Assembly,
        typeof(MessagingApp.Web.Client._Imports).Assembly);

app.Run();
