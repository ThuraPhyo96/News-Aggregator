# --------------------
# Stage 1: Build
# --------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all files from repo (Render will use root as context via .render.yaml)
COPY . .

# Restore and publish
RUN dotnet restore "News Aggregator.sln"
RUN dotnet publish "src/Services/NewsService/News.API/News.API.csproj" -c Release -o /app/publish --no-restore

# --------------------
# Stage 2: Runtime
# --------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "News.API.dll"]
