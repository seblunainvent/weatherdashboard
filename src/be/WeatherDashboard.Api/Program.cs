using WeatherDashboard.Api.Caching;
using WeatherDashboard.Api.Common.ErrorHandling.WeatherDashboard.Api.Common.Extensions;
using WeatherDashboard.Api.Common.Extensions;
using WeatherDashboard.Core.Services;
using WeatherDashboard.Core.User;
using WeatherDashboard.DataProviders.OpenWeatherMap.Extensions;

namespace WeatherDashboard.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddWeatherProviderExceptionHandling();
        builder.Services.AddGlobalExceptionHandling();
        
        builder.Services.AddResponseCaching();

        builder.Services.ConfigureOpenWeatherMapProvider(builder.Configuration)
            .AddOpenWeatherMapProvider()
            .AddOpenWeatherMapCoordinatesProvider();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowMyLittleReactFrontendApp",
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173") // React dev server
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        builder.Services.AddTransient<ILocationCoordinatesCache, MemoryLocationCoordinatesCache>();
        builder.Services.AddTransient<IWeatherService, WeatherService>();
        builder.Services.AddSingleton<IUserStorage, InMemoryUserStorage>();
        
        builder.Services.AddMemoryCache();
        
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseResponseCaching();

        app.UseCors("AllowMyLittleReactFrontendApp");
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseWeatherProviderExceptionHandling();
        app.UseGlobalExceptionHandling();
        app.UseAuthorization();


        app.MapControllers();
       
        app.Run();
    }
}