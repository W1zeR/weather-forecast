namespace WeatherForecast.Models
{
    public class WeatherViewModel
    {
        public DateOnly Date { get; set; }
        public decimal Temperature { get; set; }
        public decimal WindSpeed { get; set; }
    }
}
