# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Go 3 levels up to reach repo root
COPY ../../../.. .

# Restore and publish
RUN dotnet restore "News Aggregator.sln"
RUN dotnet publish "src/Services/NewsService/News.API/News.API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "News.API.dll"]
