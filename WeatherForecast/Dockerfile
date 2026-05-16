FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80
ENV ApiKeys__Geoapify=""
ENV ApiKeys__OpenWeatherMap=""
ENV ApiKeys__WeatherApi=""
ENV APPLICATIONINSIGHTS_CONNECTION_STRING=""
ENV ASPNETCORE_ENVIRONMENT="Production"

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WeatherForecast.csproj", "./"]
RUN dotnet restore "./WeatherForecast.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "WeatherForecast.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeatherForecast.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeatherForecast.dll"]