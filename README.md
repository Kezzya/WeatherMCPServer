Real Weather MCP Server
This is a .NET MCP (Model Context Protocol) server that provides real-time weather data using the OpenWeatherMap API. It supports queries for current weather, 3-day forecasts, and weather alerts for cities worldwide.
Overview
The Real Weather MCP Server integrates with the OpenWeatherMap API to provide accurate weather information through MCP tools. It is built using .NET 8.0 and the Microsoft.Extensions.AI.Abstractions library, following .NET best practices for code quality, error handling, and logging.
Prerequisites

.NET SDK: Version 8.0 or later.
OpenWeatherMap API Key: Obtain a free API key from OpenWeatherMap.
Visual Studio or Visual Studio Code: For development and testing.
(Optional) An MCP client or AI assistant (e.g., Claude) to interact with the server.

Setup Instructions

Clone the Repository:
git clone <repository-url>
cd WeatherMcpServer


Install the MCP Server Template (if not already installed):
dotnet new install Microsoft.Extensions.AI.Templates


Restore Dependencies:Ensure the required NuGet packages are installed by running:
dotnet restore


Configure the API Key:

Option 1: Environment Variable:Set the OpenWeatherMap__ApiKey environment variable:export OpenWeatherMap__ApiKey="your-api-key-here"  # Linux/macOS
set OpenWeatherMap__ApiKey=your-api-key-here       # Windows


Option 2: appsettings.json:Create or update appsettings.json in the project root:{
  "OpenWeatherMap": {
    "ApiKey": "your-api-key-here"
  }
}




Run the Server:
dotnet run



Usage
The server exposes three MCP tools, which can be invoked via an MCP client or AI assistant:

GetCurrentWeather:

Description: Retrieves current weather conditions for a specified city.
Parameters:
city (string): The city name (e.g., "London").
countryCode (string, optional): The country code (e.g., "UK").


Example:GetCurrentWeather("London", "UK")

Sample Output:Current weather in London: clear sky, Temperature: 15.2°C, Feels like: 14.8°C, Humidity: 65%




GetWeatherForecast:

Description: Retrieves a 3-day weather forecast for a specified city.
Parameters:
city (string): The city name (e.g., "Tokyo").
countryCode (string, optional): The country code (e.g., "JP").


Example:GetWeatherForecast("Tokyo", "JP")

Sample Output:3-day forecast for Tokyo:
2025-08-05: clear sky, Temp: 28.5°C
2025-08-06: light rain, Temp: 26.3°C
2025-08-07: cloudy, Temp: 27.1°C




GetWeatherAlerts (Bonus):

Description: Retrieves weather alerts for a specified city, if available.
Parameters:
city (string): The city name (e.g., "New York").
countryCode (string, optional): The country code (e.g., "US").


Example:GetWeatherAlerts("New York", "US")

Sample Output:No weather alerts for New York at this time.





Testing
Unit tests are provided in the WeatherMcpServer.Tests project using xUnit. To run the tests:
dotnet test

The tests cover:

Successful weather data retrieval.
Error handling for invalid cities.
API key configuration validation.

Troubleshooting

API Key Errors: Ensure the OpenWeatherMap__ApiKey environment variable or appsettings.json is correctly configured.
HTTP Errors: Check your internet connection and the OpenWeatherMap API status.
Tool Not Found: Verify that WeatherTools.cs is in the Tools folder and that [McpServerTool] attributes are applied.

Project Structure
WeatherMcpServer/
├── Tools/
│   └── WeatherTools.cs
├── Tests/
│   └── WeatherToolsTests.cs
├── appsettings.json
├── Program.cs
├── WeatherMcpServer.csproj
├── README.md
