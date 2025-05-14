using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using News.API.Authorization;
using News.Application.Interfaces;
using News.Application.Services;
using News.Infrastructure;
using NewsAggregator.API.Authorization;
using Notification.Infrastructure.Messaging;
using Serilog;
using System.Text;
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

            builder.Services.AddNewsInfrastructureServices(builder.Configuration);
            builder.Services.AddScoped<INewsAppService, NewsAppService>();
            builder.Services.AddScoped<NewsStorageAppService>();
            builder.Services.AddScoped<IArticleEventPublisher, ArticleEventPublisher>();

            builder.Services.AddControllers();

            builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],          
                    ValidAudience = builder.Configuration["Jwt:Audience"],      
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            Environment.GetEnvironmentVariable("JWT_Key") ?? builder.Configuration["Jwt:Key"]!
                        )
                    )
                };
            });
            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "News Aggregator API", Version = "v1" });

                // Add JWT bearer auth definition
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
                });

                // Add security requirement to use the scheme globally
                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

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

            builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddSingleton<IAuthorizationPolicyProvider>(sp =>
                new DynamicPermissionPolicyProvider(
                    sp,
                    sp.GetRequiredService<IOptions<AuthorizationOptions>>(),
                    sp.GetRequiredService<IMemoryCache>()
                )
            );

            var app = builder.Build();
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NewsAPI v1");
                });
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
            Log.Fatal(ex, "NewsAPI terminated unexpectedly!");
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