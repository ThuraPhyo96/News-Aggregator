using NewsAggregator.Application.Interfaces;
using NewsAggregator.Application.Services;
using NewsAggregator.Infrastructure;
using NewsAggregator.Infrastructure.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register Infrastructure Services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<INewsAppService, NewsAppService>();
builder.Services.AddScoped<NewsStorageAppService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) 
{
    //var newsService = scope.ServiceProvider.GetRequiredService<NewsApiClient>();
    //await newsService.FetchAndStoreNewsAsync(q: "bitcoin");
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
