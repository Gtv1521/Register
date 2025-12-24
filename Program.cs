using FrameworkDriver_Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure DataContext with connection strings from appsettings.json
builder.Services.Configure<DataContext>(builder.Configuration.GetSection("ConnectionStrings"));

// iniciacion de servicios externos
builder.Services.AddScoped<DataContext>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
