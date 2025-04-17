# Step 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln ./
COPY src/NewsAggregator.API/NewsAggregator.API.csproj ./src/NewsAggregator.API/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Publish the app to the /out folder
WORKDIR /app/src/NewsAggregator.API
RUN dotnet publish -c Release -o /out

# Step 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published app from build stage
COPY --from=build /out .

# Expose port 80 for HTTP
EXPOSE 80

# Set entry point
ENTRYPOINT ["dotnet", "NewsAggregator.API.dll"]