using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"ocelot.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Ocelot + Swagger
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add JWT Authentication BEFORE app.Build()
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
                Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("JWT_Key") ?? builder.Configuration["Jwt:Key"]!
                )
            )
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
app.UseSwagger();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

await app.UseOcelot();

app.Run();
