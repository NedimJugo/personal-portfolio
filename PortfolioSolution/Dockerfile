FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY PortfolioSolution.sln ./

# Copy project files - use ACTUAL folder name
COPY PortfolioApi/*.csproj ./PortfolioApi/
COPY Portfolio.Services/*.csproj ./Portfolio.Services/
COPY Portfolio.Models/*.csproj ./Portfolio.Models/

# Restore dependencies
RUN dotnet restore PortfolioSolution.sln

# Copy everything else
COPY . .

# Build and publish - use ACTUAL project path   
RUN dotnet publish PortfolioApi/Portfolio.WebAPI.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Render's containers have a low inotify watch limit; ASP.NET Core's default
# appsettings.json hot-reload watcher can exhaust it and crash the app on startup
# ("configured user limit (128) on the number of inotify instances"). Not needed
# in a deployed container, so disable it.
ENV DOTNET_hostBuilder:reloadConfigOnChange=false

ENTRYPOINT ["dotnet", "Portfolio.WebAPI.dll"]