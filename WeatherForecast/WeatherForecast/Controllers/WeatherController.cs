using Microsoft.AspNetCore.Mvc;
using WeatherForecast.Models;
using WeatherForecast.Services;

namespace WeatherForecast.Controllers
{
    [ApiController]
    [Route("/weather/forecast")]
    public class WeatherController(IWeatherService weatherForecastService) : ControllerBase
    {
        private const decimal MinTemperature = -90m, MaxTemperature = 60m;
        private const decimal MinWind = 0m, MaxWind = 150m;

        [HttpGet("{date}")]
        [ProducesResponseType(typeof(WeatherViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WeatherViewModel>> Get(DateOnly date, CancellationToken ct)
        {
            var forecast = await weatherForecastService.GetByDateAsync(date, ct);

            if (forecast is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Прогноз не найден",
                    Detail = $"Прогноза на {date:yyyy-MM-dd} нет."
                });
            }

            return Ok(forecast);
        }

        [HttpPost]
        [ProducesResponseType(typeof(WeatherViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<WeatherViewModel>> Create([FromBody] WeatherViewModel weather, CancellationToken ct)
        {
            Validate(weather);

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await weatherForecastService.AddAsync(weather, ct);

            if (result == AddForecastResult.AlreadyExists)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Запись уже существует",
                    Detail = $"Прогноз на {weather.Date:yyyy-MM-dd} уже добавлен."
                });
            }

            return CreatedAtAction(nameof(Get), new { date = weather.Date.ToString("yyyy-MM-dd") }, weather);
        }

        private void Validate(WeatherViewModel w)
        {
            if (w.Date == default)
                ModelState.AddModelError(nameof(w.Date), "Дата не указана или некорректна (ожидается yyyy-MM-dd).");

            if (w.Temperature < MinTemperature || w.Temperature > MaxTemperature)
                ModelState.AddModelError(nameof(w.Temperature),
                    $"Температура должна быть от {MinTemperature} до {MaxTemperature} °C.");

            if (w.WindSpeed < MinWind || w.WindSpeed > MaxWind)
                ModelState.AddModelError(nameof(w.WindSpeed),
                    $"Скорость ветра должна быть от {MinWind} до {MaxWind} м/с.");
        }
    }
}
