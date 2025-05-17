# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# 1. First copy ALL required project files for restore
COPY ["src/Shared/NewsAggregator.Contracts/NewsAggregator.Contracts.csproj", "src/Shared/NewsAggregator.Contracts/"]
COPY ["src/Services/NewsService/News.API/News.API.csproj", "src/Services/NewsService/News.API/"]
COPY ["src/Services/NewsService/News.Application/News.Application.csproj", "src/Services/NewsService/News.Application/"]
COPY ["src/Services/NewsService/News.Domain/News.Domain.csproj", "src/Services/NewsService/News.Domain/"] 
COPY ["src/Services/NewsService/News.Infrastructure/News.Infrastructure.csproj", "src/Services/NewsService/News.Infrastructure/"]

# 2. Restore dependencies
RUN dotnet restore "src/Services/NewsService/News.API/News.API.csproj"

# 3. Now copy everything else (simplified path)
COPY . .

# 4. Publish the API project
RUN dotnet publish "src/Services/NewsService/News.API/News.API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "News.API.dll"]