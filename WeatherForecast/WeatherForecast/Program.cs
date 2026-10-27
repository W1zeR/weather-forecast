using WeatherForecast.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.WriteIndented = true;
    });

services.AddSwaggerGen();

services.AddSingleton<IWeatherService, WeatherService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
