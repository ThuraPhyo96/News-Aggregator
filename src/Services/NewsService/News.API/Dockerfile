# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything from the full repo root (go up 4 levels)
COPY ../../../../.. .

RUN dotnet restore "src/Services/NewsService/News.API/News.API.csproj"

RUN dotnet publish "src/Services/NewsService/News.API/News.API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "News.API.dll"]