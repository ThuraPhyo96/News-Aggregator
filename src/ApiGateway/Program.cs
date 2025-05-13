using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Ocelot + Swagger
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger for Ocelot
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/news/swagger/v1/swagger.json", "News API V1");
    c.RoutePrefix = "swagger";
});


// Ocelot middleware
await app.UseOcelot();
app.Run();
