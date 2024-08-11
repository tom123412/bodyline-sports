using Azure.Monitor.OpenTelemetry.AspNetCore;
using bodyline_sports.Components;
using bodyline_sports.Options;
using bodyline_sports.Services;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenTelemetry().UseAzureMonitor();
builder.Services.AddMemoryCache();

builder.Services.Configure<FacebookOptions>(
    builder.Configuration.GetSection(
        key: nameof(FacebookOptions)));

builder.Services.AddHttpClient("Facebook", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://graph.facebook.com/v20.0/");
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/json");
});
builder.Services.AddScoped<IFacebook, Facebook>();


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
