using Blazored.LocalStorage;
using Microsoft.AspNetCore.Http.Features;
using Wallman.Components;
using Wallman.Components.Pages;

namespace Wallman;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.Configure<FormOptions>(options => {
            options.MultipartBodyLengthLimit = 20 * 1024 * 1024;
        });
        builder.Services.Configure<AdminLoginModel>(builder.Configuration.GetSection("Wallman_admin"));


        builder.Services.AddServerSideBlazor()
            .AddHubOptions(options => {
                options.MaximumReceiveMessageSize = 20 * 1024 * 1024;
            });

        
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();
        
        app.MapControllers();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}