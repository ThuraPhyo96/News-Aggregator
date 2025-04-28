using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewsAggregator.API.Authorization;
using NewsAggregator.API.Middleware;
using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Services;
using NewsAggregator.Infrastructure;
using NewsAggregator.Infrastructure.Data;
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

            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddScoped<INewsAppService, NewsAppService>();
            builder.Services.AddScoped<IUserAppService, UserAppService>();
            builder.Services.AddScoped<NewsStorageAppService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // JWT Auth Setup
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_Key") ?? builder.Configuration["Jwt:Key"]!))
                    };
                });

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
                options.AddFixedWindowLimiter(policyName: "fixed", configureOptions: limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5; // Max 5 requests
                    limiterOptions.Window = TimeSpan.FromMinutes(1); // Every 1 minute
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 2; // Allow 2 extra requests to queue
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
