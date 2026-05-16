# WeatherForecast
Can be found under the link: https://weather-app.mangobush-39b0e5b2.westeurope.azurecontainerapps.io/swagger/index.html
Dockerized .NET 10 Web API that returns weather forecasts for a given **city**, **country** and **date**.

The service aggregates results from free providers and caches responses to reduce external API calls.

## What the API does

- Single endpoint: `GET /Weather`
- Input query parameters:
  - `city`
  - `country`
  - `date`
- Output:
  - a collection of forecasts from available providers

## Behavior and validation

### `400 Bad Request`
The API returns `400` when the request is invalid:
- the date is in the past
- the date is more than 6 days in the future
- the location cannot be resolved to coordinates

For invalid location input, the response message is:

> `Unable to find coordinates for the specified location. Please check the city '...' and country '...'.`

### `500 Internal Server Error`
The API returns `500` when:

- the location was resolved successfully, but **none** of the weather providers returned a forecast

## Date limits

The `date` value must be:

- today or later
- at most **6 days ahead**

## Cache

- Weather results are cached in memory for **30 minutes**
- Cache key is based on `city`, `country` and `date`
- The cache helps avoid repeated calls to third-party services for the same request

## Data sources

The app uses the following external services:

- [Geoapify](https://www.geoapify.com/) — geocoding (city/country to coordinates)
- [WeatherAPI](https://www.weatherapi.com/)
- [OpenWeatherMap](https://openweathermap.org/)
- [Open-Meteo](https://open-meteo.com/)

## Architecture

- `WeatherController` is a thin HTTP endpoint
- `GetWeatherQueryHandler` contains the query/business logic
- Providers are executed in parallel and their non-null forecasts are aggregated into one result

## Docker

The repository contains a `Dockerfile` for building the API image.

## CI/CD and deployment

The repository contains a GitHub Actions workflow:

- `.github/workflows/azure-container-app.yml`

What it does:

1. Builds the Docker image from `./WeatherForecast`
2. Pushes the image to Docker Hub
3. Deploys/updates the image in **Azure Container Apps**
