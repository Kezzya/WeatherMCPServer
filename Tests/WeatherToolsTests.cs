using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeatherMcpServer.Tools;
using Xunit;

namespace WeatherMcpServer.Tests
{
    public class WeatherToolsTests
    {
        private readonly WeatherTools _weatherTools;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IConfiguration _configuration;

        public WeatherToolsTests()
        {
            // Mock configuration
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "OpenWeatherMap:ApiKey", "test-api-key" }
                })
                .Build();
            _configuration = config;

            // Mock HttpClient
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Mock logger
            var logger = Mock.Of<ILogger<WeatherTools>>();

            // Initialize WeatherTools
            _weatherTools = new WeatherTools(httpClient, logger, _configuration);
        }

        [Fact]
        public async Task GetCurrentWeather_ValidCity_ReturnsWeatherData()
        {
            // Arrange
            var jsonResponse = @"{
                ""name"": ""London"",
                ""main"": { ""temp"": 15.2, ""feels_like"": 14.8, ""humidity"": 65 },
                ""weather"": [ { ""description"": ""clear sky"" } ]
            }";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _weatherTools.GetCurrentWeather("London", "UK");

            // Assert
            Assert.Contains("Current weather in London: clear sky", result);
            Assert.Contains("Temperature: 15.2°C", result);
            Assert.Contains("Feels like: 14.8°C", result);
            Assert.Contains("Humidity: 65%", result);
        }

        [Fact]
        public async Task GetCurrentWeather_InvalidCity_ReturnsErrorMessage()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("{\"message\": \"city not found\"}", Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _weatherTools.GetCurrentWeather("InvalidCity");

            // Assert
            Assert.Equal("Error: Unable to connect to the weather service.", result);
        }

        [Fact]
        public async Task GetWeatherForecast_ValidCity_ReturnsForecastData()
        {
            // Arrange
            var jsonResponse = @"{
                ""city"": { ""name"": ""Tokyo"" },
                ""list"": [
                    { ""dt_txt"": ""2025-08-05 12:00:00"", ""main"": { ""temp"": 28.5 }, ""weather"": [ { ""description"": ""clear sky"" } ] },
                    { ""dt_txt"": ""2025-08-06 12:00:00"", ""main"": { ""temp"": 26.3 }, ""weather"": [ { ""description"": ""light rain"" } ] },
                    { ""dt_txt"": ""2025-08-07 12:00:00"", ""main"": { ""temp"": 27.1 }, ""weather"": [ { ""description"": ""cloudy"" } ] }
                ]
            }";
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _weatherTools.GetWeatherForecast("Tokyo", "JP");

            // Assert
            Assert.Contains("3-day forecast for Tokyo", result);
            Assert.Contains("2025-08-05: clear sky, Temp: 28.5°C", result);
            Assert.Contains("2025-08-06: light rain, Temp: 26.3°C", result);
            Assert.Contains("2025-08-07: cloudy, Temp: 27.1°C", result);
        }

        [Fact]
        public async Task GetWeatherAlerts_NoAlerts_ReturnsNoAlertsMessage()
        {
            // Arrange
            var coordResponse = @"{
                ""name"": ""New York"",
                ""coord"": { ""lat"": 40.7128, ""lon"": -74.0060 }
            }";
            var oneCallResponse = @"{
                ""alerts"": []
            }";
            _httpMessageHandlerMock
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(coordResponse, Encoding.UTF8, "application/json")
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(oneCallResponse, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _weatherTools.GetWeatherAlerts("New York", "US");

            // Assert
            Assert.Equal("No weather alerts for New York at this time.", result);
        }

        [Fact]
        public void Constructor_MissingApiKey_ThrowsException()
        {
            // Arrange
            var emptyConfig = new ConfigurationBuilder().Build();
            var httpClient = new HttpClient();
            var logger = Mock.Of<ILogger<WeatherTools>>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new WeatherTools(httpClient, logger, emptyConfig));
        }
    }
}