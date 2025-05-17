# Build Stage: Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy everything from the full repo root (go up 2 levels)
COPY ../../.. .

# Restore and publish only the project
RUN dotnet restore "src/ApiGateway/ApiGateway.csproj"
RUN dotnet publish "src/ApiGateway/ApiGateway.csproj" -c Release -o /app/publish --no-restore

# Runtime Stage: Use the official ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set the working directory
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose default port 
EXPOSE 80

# Run the API Gateway
ENTRYPOINT ["dotnet", "ApiGateway.dll"]