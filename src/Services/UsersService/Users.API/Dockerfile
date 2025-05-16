# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and all source folders
COPY . .
RUN dotnet restore "News Aggregator.sln"
RUN dotnet publish "src/Services/UsersService/Users.API/Users.API.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "Users.API.dll"]
