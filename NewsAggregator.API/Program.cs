using NewsAggregator.API.Middleware;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Services;
using NewsAggregator.Infrastructure;
using NewsAggregator.Infrastructure.HttpClients;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    // Add services to the container.

    builder.Host.UseSerilog();  // This tells ASP.NET Core to use Serilog for logging

    // Register Serilog via Microsoft's ILogger
    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(Log.Logger);
    });

    // Register Infrastructure Services
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddScoped<INewsAppService, NewsAppService>();
    builder.Services.AddScoped<NewsStorageAppService>();

    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Middleware
    app.UseSerilogRequestLogging(); // Logs all HTTP requests
    // Register custom middleware BEFORE other middlewares like endpoints
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    using (var scope = app.Services.CreateScope())
    {
        var newsService = scope.ServiceProvider.GetRequiredService<NewsApiClient>();
        await newsService.FetchAndStoreNewsAsync(q: "bitcoin");
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NewsAPI terminated unexpectedly!");
}
finally
{
    Log.CloseAndFlush();
}