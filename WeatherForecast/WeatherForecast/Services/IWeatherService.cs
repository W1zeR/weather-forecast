using WeatherForecast.Models;

namespace WeatherForecast.Services
{
    public interface IWeatherService
    {
        Task<WeatherViewModel?> GetByDateAsync(DateOnly date, CancellationToken ct = default);
        Task<AddForecastResult> AddAsync(WeatherViewModel weather, CancellationToken ct = default);
    }
}
