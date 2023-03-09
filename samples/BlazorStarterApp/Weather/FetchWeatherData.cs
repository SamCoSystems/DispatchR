namespace BlazorStarterApp.Weather;

using DispatchR;
using System.Threading.Tasks;

public static class FetchWeatherData
{
	private static readonly string[] Summaries = new[]
	{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

	public record Request(DateTime StartDate);
	public record Result(List<WeatherForecast> WeatherForecasts);

	public class Handler : IAsyncNotificationHandler<Request>
	{
		private readonly IDispatcher _dispatcher;

		public Handler(IDispatcher dispatcher)
			=> _dispatcher = dispatcher;

		public async Task HandleAsync(Request notification)
		{
			await Task.Delay(2000);

			var forecasts = Enumerable.Range(1, 5)
				.Select(index => new WeatherForecast(
					Date: notification.StartDate.AddDays(index),
					TemperatureC: Random.Shared.Next(-20, 55),
					Summary: Summaries[Random.Shared.Next(Summaries.Length)]))
				.ToList();

			var result = new Result(forecasts);
			_dispatcher.Dispatch(result);
		}
	}
}
