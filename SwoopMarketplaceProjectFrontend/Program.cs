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

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();


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

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
