using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
namespace GHLearning.OpenTelemetrySample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController(
	ILogger<WeatherForecastController> logger) : ControllerBase
{
	private static readonly string[] _Summaries =
	[
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	];

	[HttpGet]
	public IEnumerable<WeatherForecast> Get()
	{
		IEnumerable<WeatherForecast> weatherForecasts = [.. Enumerable.Range(1, 5).Select(index => new WeatherForecast
		{
			Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
			TemperatureC = Random.Shared.Next(-20, 55),
			Summary = _Summaries[Random.Shared.Next(_Summaries.Length)]
		})];

		logger.LogInformation("Weather forecast generated: {WeatherForecasts}", JsonSerializer.Serialize(weatherForecasts));

		return weatherForecasts;
	}
}