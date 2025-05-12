using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Users.API.Authorization;
using Users.API.Middleware;
using Users.Application.Interfaces;
using Users.Application.Services;
using Users.Infrastructure;
using Users.Infrastructure.Data;
using Serilog;
using System.Threading.RateLimiting;

public partial class Program
{
    public static async Task<WebApplication> CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            builder.Host.UseSerilog();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(Log.Logger);
            });

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddScoped<IUserAppService, UserAppService>();
            builder.Services.AddScoped<ITokenAppService, TokenAppService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "Users Management API", Version = "v1" });
            });

            builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider>(sp =>
                new DynamicPermissionPolicyProvider(
                    sp,
                    sp.GetRequiredService<IOptions<AuthorizationOptions>>(),
                    sp.GetRequiredService<IMemoryCache>()
                )
            );
            builder.Services.AddMemoryCache();

            // Add Rate Limiting services
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // Set global status code for rejected requests

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";

                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterVal)
                        ? retryAfterVal.TotalSeconds
                        : 60; // Default fallback

                    var errorResponse = new
                    {
                        error = "Too many requests. Please try again later.",
                        retryAfterSeconds = retryAfter
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
                };

                options.AddFixedWindowLimiter(policyName: "fixed", configureOptions: limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5; // Max 5 requests
                    limiterOptions.Window = TimeSpan.FromMinutes(1); // Every 1 minute
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0; // Disable queueing: reject immediately
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<SeedData>();
                await seeder.SeedAsync();
            }

            app.UseSerilogRequestLogging();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            // Enable Rate Limiting
            app.UseRateLimiter();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Users API terminated unexpectedly!");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static async Task Main(string[] args)
    {
        var app = await CreateApp(args);
        app.Run();
    }
}