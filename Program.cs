using Azure.Monitor.OpenTelemetry.AspNetCore;
using bodyline_sports.Components;
using bodyline_sports.Http;
using bodyline_sports.Options;
using bodyline_sports.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Net.Http.Headers;
using FacebookOptions = bodyline_sports.Options.FacebookOptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenTelemetry().UseAzureMonitor();
builder.Services.AddMemoryCache();

builder.Services.Configure<FacebookOptions>(builder.Configuration.GetSection(key: nameof(FacebookOptions)));
builder.Services.Configure<ContactOptions>(builder.Configuration.GetSection(key: nameof(ContactOptions)));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = FacebookDefaults.AuthenticationScheme;
    })
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration["FacebookOptions:AppId"] ?? facebookOptions.AppId;
        facebookOptions.AppSecret = builder.Configuration["FacebookOptions:AppSecret"] ?? facebookOptions.AppSecret;
        facebookOptions.Events = new()
        {
            OnRedirectToAuthorizationEndpoint = (context) =>
            {
                context.Response.Redirect($"{context.RedirectUri}&config_id={builder.Configuration["FacebookOptions:ConfigId"]}");
                return Task.CompletedTask;
            }
        };
    })
    .AddCookie()
    .AddIdentityCookies()
    ;

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services
    .AddHttpClient("Facebook", (httpClient) =>
    {
        httpClient.BaseAddress = new Uri("https://graph.facebook.com/v20.0/");
        httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
    })
    .AddHttpMessageHandler<AddAuthorisationHeaderHandler>()
    ;

builder.Services.AddTransient<AddAuthorisationHeaderHandler>();
builder.Services.AddScoped<IFacebook, Facebook>();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services
    .AddRazorComponents()
    //.AddInteractiveServerComponents()
    ;

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

app
    .MapRazorComponents<App>()
    //.AddInteractiveServerRenderMode()
    ;

app.Run();
