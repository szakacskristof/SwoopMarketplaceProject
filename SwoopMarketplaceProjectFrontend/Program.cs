using Microsoft.AspNetCore.Authentication.Cookies;
using SwoopMarketplaceProjectFrontend.Infrastructure;
using SwoopMarketplaceProjectFrontend.Services;

namespace SwoopMarketplaceProjectFrontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddHttpClient("SwoopApi", c =>
            {
                c.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            new HttpClientHandler { UseProxy = false }
            );

            builder.Services.AddScoped<ListingApi>();
            builder.Services.AddScoped<CategoryApi>();
            builder.Services.AddScoped<ListingViewApi>();
            builder.Services.AddScoped<ListingImageApi>();
            builder.Services.AddScoped<UserApi>();
            builder.Services.AddScoped<ReportApi>();
            builder.Services.AddScoped<AdminApi>(); // <-- Registered AdminApi

            // Register bookmark service so PageModels that depend on it can be activated.
            builder.Services.AddScoped<BookmarkApi>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<AuthSession>();
            builder.Services.AddScoped<AuthApi>();
            builder.Services.AddTransient<JwtBearerHandler>();

            builder.Services.AddHttpClient("SwoopApi", c =>
            {
                c.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            new HttpClientHandler { UseProxy = false }
            )
            .AddHttpMessageHandler<JwtBearerHandler>();

            builder.Services.AddScoped<AuthPageFilter>();

            // Register authentication (default scheme) so UseAuthorization has a challenge scheme.
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Errors/Forbidden";
                });

            // Add services to the container.
            builder.Services.AddRazorPages()
            .AddMvcOptions(options =>
            {
                options.Filters.AddService<AuthPageFilter>();

            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            // Ensure authentication middleware runs before authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
