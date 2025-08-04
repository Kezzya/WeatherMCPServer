# Real Weather MCP Server

This is a .NET-based MCP (Model Context Protocol) server that provides real-time weather data using the [OpenWeatherMap API](https://openweathermap.org/). It supports queries for current weather, 3-day forecasts, and weather alerts for cities worldwide, integrated with AI assistants like Claude via the MCP protocol.

## Overview

The `Real Weather MCP Server` is built using .NET 8.0 and the `Microsoft.Extensions.AI.Abstractions` library. It follows .NET best practices, including dependency injection, error handling, and logging. The server exposes three tools:
- `GetCurrentWeather`: Retrieves current weather conditions.
- `GetWeatherForecast`: Provides a 3-day weather forecast.
- `GetWeatherAlerts`: Fetches active weather alerts (if available).

The server communicates with MCP clients (e.g., Claude Desktop) via `stdio` (standard input/output).

## Prerequisites

- **.NET SDK**: Version 8.0 or later ([Download](https://dotnet.microsoft.com/download/dotnet/8.0)).
- **OpenWeatherMap API Key**: Obtain a free API key from [OpenWeatherMap](https://openweathermap.org/api).
- **Visual Studio or Visual Studio Code**: For development and testing.
- **MCP Client (Optional)**: An AI assistant like Claude Desktop to interact with the server.
- **Git**: For cloning the repository.

## Setup Instructions

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Kezzya/WeatherMcpServer.git
   cd WeatherMcpServer
   ```

2. **Install the MCP Server Template** (if not already installed):
   ```bash
   dotnet new install Microsoft.Extensions.AI.Templates
   ```

3. **Restore Dependencies**:
   Ensure all required NuGet packages are installed:
   ```bash
   dotnet restore
   ```

4. **Configure the API Key**:
   You need to provide an OpenWeatherMap API key. You can do this in one of two ways:

   **1: Environment Variable**:
   Set the `OpenWeatherMap__ApiKey` environment variable:
   ```bash
   # Linux/macOS
   export OpenWeatherMap__ApiKey="your-api-key-here"
   # Windows
   set OpenWeatherMap__ApiKey=your-api-key-here
   ```

5. **Build the Project**:
   ```bash
   dotnet build
   ```

6. **(Optional) Create Executable**:
   To generate a standalone executable for use with `stdio`:
   ```bash
   dotnet publish -c Release -o bin/Release/net8.0
   ```

7. **Configure Claude Desktop**:
   Update the Claude configuration file (`C:\Users\<your-username>\AppData\Roaming\Claude\claude_desktop_config.json` on Windows) to include the MCP server:
   ```json
   {
     "mcpServers": {
       "weather-mcp-server": {
         "type": "stdio",
         "command": "C:\\Users\\<your-username>\\source\\repos\\WeatherMcpServer\\WeatherMcpServer\\bin\\Release\\net8.0\\WeatherMcpServer.exe",
         "args": [],
         "env": {
           "OpenWeatherMap__ApiKey": "your-api-key-here"
         }
       }
     }
   }
   ```
   - Replace `<your-username>` with your actual Windows username.
   - Replace `your-api-key-here` with your OpenWeatherMap API key.
   - Ensure the path to `WeatherMcpServer.exe` is correct.

8. **Run the Server**:
   If testing manually:
   ```bash
   dotnet run
   ```
   Or, if using the `.exe` with Claude:
   ```bash
   C:\Users\<your-username>\source\repos\WeatherMcpServer\WeatherMcpServer\bin\Release\net8.0\WeatherMcpServer.exe
   ```

## Usage

The server exposes three MCP tools that can be invoked via an MCP client (e.g., Claude Desktop):

### `GetCurrentWeather`
- **Description**: Retrieves current weather conditions for a specified city.
- **Parameters**:
  - `city` (string): The city name (e.g., "London").
  - `countryCode` (string, optional): The country code (e.g., "UK").
- **Example**:
  ```
  GetCurrentWeather("London", "UK")
  ```
- **Sample Output**:
  ```
  Current weather in London: clear sky, Temperature: 15.2°C, Feels like: 14.8°C, Humidity: 65%
  ```

### `GetWeatherForecast`
- **Description**: Retrieves a 3-day weather forecast for a specified city.
- **Parameters**:
  - `city` (string): The city name (e.g., "Tokyo").
  - `countryCode` (string, optional): The country code (e.g., "JP").
- **Example**:
  ```
  GetWeatherForecast("Tokyo", "JP")
  ```
- **Sample Output**:
  ```
  3-day forecast for Tokyo:
  2025-08-05: clear sky, Temp: 28.5°C
  2025-08-06: light rain, Temp: 26.3°C
  2025-08-07: cloudy, Temp: 27.1°C
  ```

### `GetWeatherAlerts`
- **Description**: Retrieves active weather alerts for a specified city, if available.
- **Parameters**:
  - `city` (string): The city name (e.g., "New York").
  - `countryCode` (string, optional): The country code (e.g., "US").
- **Example**:
  ```
  GetWeatherAlerts("New York", "US")
  ```
- **Sample Output**:
  ```
  No weather alerts for New York at this time.
  ```

## Testing

Unit tests are provided in the `WeatherMcpServer.Tests` project using xUnit. To run the tests:
```bash
cd WeatherMcpServer.Tests
dotnet test
```

The tests cover:
- Successful weather data retrieval for valid cities.
- Error handling for invalid cities or API keys.
- Validation of API key configuration.

## Troubleshooting

- **API Key Errors**:
  - Ensure the `OpenWeatherMap__ApiKey` is set in `appsettings.json` or as an environment variable.
  - Verify the key by making a direct API call:
    ```bash
    curl "https://api.openweathermap.org/data/2.5/weather?q=Moscow,RU&appid=your-api-key-here&units=metric"
    ```

- **Claude JSON Errors**:
  - If you see errors like `Unexpected token 'H', "Hosting en"... is not valid JSON`, ensure `appsettings.json` disables console logging (see above).
  - Check the Claude logs:
    ```bash
    type %APPDATA%\Claude\Logs\mcp*.log | more
    ```

- **Tool Not Found**:
  - Verify that `WeatherTools.cs` is in the `Tools` folder and has `[McpServerTool]` attributes.
  - Ensure the server is running before starting Claude.

- **Build Errors**:
  - Run `dotnet restore` to install missing NuGet packages.
  - Check `WeatherMcpServer.csproj` for required packages:
    ```xml
    <ItemGroup>
      <PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="9.0.0-preview.*" />
      <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
      <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
      <PackageReference Include="System.Text.Json" Version="8.0.0" />
    </ItemGroup>
    ```


## Contributing

Contributions are welcome! Please submit a pull request or open an issue on [GitHub](https://github.com/Kezzya/WeatherMcpServer).
 