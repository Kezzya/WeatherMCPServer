using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WeatherMcpServer.Tools
{
    public class WeatherTools
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeatherTools> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.openweathermap.org/data/2.5";

        public WeatherTools(HttpClient httpClient, ILogger<WeatherTools> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["OpenWeatherMap:ApiKey"]
                ?? throw new InvalidOperationException("OpenWeatherMap API key not configured. Set the 'OpenWeatherMap:ApiKey' environment variable or add it to appsettings.json.");
        }

        [McpServerTool]
        [Description("Gets current weather conditions for the specified city.")]
        public async Task<string> GetCurrentWeather(
            [Description("The city name to get weather for")] string city,
            [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
        {
            try
            {
                _logger.LogInformation("Fetching current weather for {City}, {CountryCode}", city, countryCode ?? "N/A");

                string location = countryCode != null ? $"{city},{countryCode}" : city;
                string url = $"{_baseUrl}/weather?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var weatherData = JsonSerializer.Deserialize<WeatherResponse>(json);

                if (weatherData == null || weatherData.Main == null || weatherData.Weather == null)
                {
                    _logger.LogWarning("Invalid weather data received for {City}", city);
                    return "Unable to retrieve weather data.";
                }

                return $"Current weather in {weatherData.Name}: {weatherData.Weather[0].Description}, " +
                       $"Temperature: {weatherData.Main.Temp}°C, Feels like: {weatherData.Main.FeelsLike}°C, " +
                       $"Humidity: {weatherData.Main.Humidity}%";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch weather data for {City}", city);
                return "Error: Unable to connect to the weather service.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse weather data for {City}", city);
                return "Error: Invalid response from the weather service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching weather for {City}", city);
                return "Error: An unexpected error occurred.";
            }
        }

        [McpServerTool]
        [Description("Gets a 3-day weather forecast for the specified city.")]
        public async Task<string> GetWeatherForecast(
            [Description("The city name to get the forecast for")] string city,
            [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
        {
            try
            {
                _logger.LogInformation("Fetching 3-day forecast for {City}, {CountryCode}", city, countryCode ?? "N/A");

                string location = countryCode != null ? $"{city},{countryCode}" : city;
                string url = $"{_baseUrl}/forecast?q={Uri.EscapeDataString(location)}&appid={_apiKey}&units=metric";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var forecastData = JsonSerializer.Deserialize<ForecastResponse>(json);

                if (forecastData == null || forecastData.List == null || forecastData.City == null)
                {
                    _logger.LogWarning("Invalid forecast data received for {City}", city);
                    return "Unable to retrieve forecast data.";
                }

                var dailyForecasts = forecastData.List
                    .Where(f => f.DateTimeText.Contains("12:00:00")) // Noon forecasts for simplicity
                    .Take(3)
                    .Select(f => $"{f.DateTimeText.Split(' ')[0]}: {f.Weather[0].Description}, " +
                                $"Temp: {f.Main.Temp}°C")
                    .ToList();

                if (!dailyForecasts.Any())
                {
                    return "No forecast data available for the next 3 days.";
                }

                return $"3-day forecast for {forecastData.City.Name}:\n" + string.Join("\n", dailyForecasts);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch forecast data for {City}", city);
                return "Error: Unable to connect to the weather service.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse forecast data for {City}", city);
                return "Error: Invalid response from the weather service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching forecast for {City}", city);
                return "Error: An unexpected error occurred.";
            }
        }

        [McpServerTool]
        [Description("Gets weather alerts for the specified city, if available.")]
        public async Task<string> GetWeatherAlerts(
            [Description("The city name to get alerts for")] string city,
            [Description("Optional: Country code (e.g., 'US', 'UK')")] string? countryCode = null)
        {
            try
            {
                _logger.LogInformation("Fetching weather alerts for {City}, {CountryCode}", city, countryCode ?? "N/A");

                // Note: OpenWeatherMap's free tier may not include alerts in the basic /weather endpoint.
                // For this implementation, we'll use the One Call API (requires lat/lon).
                // First, get coordinates for the city.
                string location = countryCode != null ? $"{city},{countryCode}" : city;
                string coordUrl = $"{_baseUrl}/weather?q={Uri.EscapeDataString(location)}&appid={_apiKey}";

                var coordResponse = await _httpClient.GetAsync(coordUrl);
                coordResponse.EnsureSuccessStatusCode();

                var coordJson = await coordResponse.Content.ReadAsStringAsync();
                var coordData = JsonSerializer.Deserialize<WeatherResponse>(coordJson);

                if (coordData?.Coord == null)
                {
                    _logger.LogWarning("Unable to retrieve coordinates for {City}", city);
                    return "Unable to retrieve weather alerts: Invalid location.";
                }

                // Use One Call API to check for alerts
                string oneCallUrl = $"https://api.openweathermap.org/data/3.0/onecall?lat={coordData.Coord.Lat}&lon={coordData.Coord.Lon}&exclude=current,minutely,hourly,daily&appid={_apiKey}";
                var oneCallResponse = await _httpClient.GetAsync(oneCallUrl);
                oneCallResponse.EnsureSuccessStatusCode();

                var oneCallJson = await oneCallResponse.Content.ReadAsStringAsync();
                var oneCallData = JsonSerializer.Deserialize<OneCallResponse>(oneCallJson);

                if (oneCallData?.Alerts == null || !oneCallData.Alerts.Any())
                {
                    return $"No weather alerts for {city} at this time.";
                }

                var alerts = oneCallData.Alerts
                    .Select(a => $"{a.Event}: {a.Description} (from {UnixToDateTime(a.Start)} to {UnixToDateTime(a.End)})")
                    .ToList();

                return $"Weather alerts for {city}:\n" + string.Join("\n", alerts);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch weather alerts for {City}", city);
                return "Error: Unable to connect to the weather service.";
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse weather alerts for {City}", city);
                return "Error: Invalid response from the weather service.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching alerts for {City}", city);
                return "Error: An unexpected error occurred.";
            }
        }

        // Helper method to convert Unix timestamp to DateTime
        private static string UnixToDateTime(long unixTime)
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }

        // Data models for JSON deserialization
        private class WeatherResponse
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("main")]
            public MainData Main { get; set; } = new();

            [JsonPropertyName("weather")]
            public WeatherData[] Weather { get; set; } = Array.Empty<WeatherData>();

            [JsonPropertyName("coord")]
            public CoordData Coord { get; set; } = new();
        }

        private class ForecastResponse
        {
            [JsonPropertyName("list")]
            public ForecastData[] List { get; set; } = Array.Empty<ForecastData>();

            [JsonPropertyName("city")]
            public CityData City { get; set; } = new();
        }

        private class OneCallResponse
        {
            [JsonPropertyName("alerts")]
            public AlertData[] Alerts { get; set; } = Array.Empty<AlertData>();
        }

        private class MainData
        {
            [JsonPropertyName("temp")]
            public float Temp { get; set; }

            [JsonPropertyName("feels_like")]
            public float FeelsLike { get; set; }

            [JsonPropertyName("humidity")]
            public int Humidity { get; set; }
        }

        private class WeatherData
        {
            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
        }

        private class CoordData
        {
            [JsonPropertyName("lat")]
            public float Lat { get; set; }

            [JsonPropertyName("lon")]
            public float Lon { get; set; }
        }

        private class ForecastData
        {
            [JsonPropertyName("dt_txt")]
            public string DateTimeText { get; set; } = string.Empty;

            [JsonPropertyName("main")]
            public MainData Main { get; set; } = new();

            [JsonPropertyName("weather")]
            public WeatherData[] Weather { get; set; } = Array.Empty<WeatherData>();
        }

        private class CityData
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }

        private class AlertData
        {
            [JsonPropertyName("event")]
            public string Event { get; set; } = string.Empty;

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [JsonPropertyName("start")]
            public long Start { get; set; }

            [JsonPropertyName("end")]
            public long End { get; set; }
        }
    }
}