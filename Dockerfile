# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set working directory
WORKDIR /app

# Copy solution and all source folders
COPY . .

# Restore dependencies
RUN dotnet restore "News Aggregator.sln"

# Build the app
RUN dotnet publish "src/NewsAggregator.API/NewsAggregator.API.csproj" -c Release -o /app/publish --no-restore

# Use ASP.NET Core runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set working directory
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "NewsAggregator.API.dll"]