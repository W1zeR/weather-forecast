using System.Text.Json;
using WeatherForecast.Models;

namespace WeatherForecast.Services
{
    public class WeatherService(IWebHostEnvironment env) : IWeatherService
    {
        private readonly string _filePath = Path.Combine(env.ContentRootPath, "Data", "weather.json");

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web) { WriteIndented = true };

        private readonly SemaphoreSlim _lock = new(1, 1);

        public async Task<WeatherViewModel?> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);

            try
            {
                var weatherList = await LoadAsync(ct);

                return weatherList.FirstOrDefault(f => f.Date == date);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<AddForecastResult> AddAsync(WeatherViewModel weather, CancellationToken ct = default)
        {
            await _lock.WaitAsync(ct);

            try
            {
                var weatherList = await LoadAsync(ct);

                if (weatherList.Any(f => f.Date == weather.Date))
                    return AddForecastResult.AlreadyExists;

                weatherList.Add(weather);

                await SaveAsync(weatherList, ct);

                return AddForecastResult.Created;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<WeatherViewModel>> LoadAsync(CancellationToken ct)
        {
            if (!File.Exists(_filePath))
                return [];

            var json = await File.ReadAllTextAsync(_filePath, ct);

            return string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<List<WeatherViewModel>>(json, JsonOptions) ?? [];
        }

        private async Task SaveAsync(List<WeatherViewModel> items, CancellationToken ct)
        {
            var dir = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var tmp = _filePath + ".tmp";

            await File.WriteAllTextAsync(tmp, JsonSerializer.Serialize(items, JsonOptions), ct);

            File.Move(tmp, _filePath, overwrite: true);
        }
    }
}
