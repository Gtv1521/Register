using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Repositories;
using FrameworkDriver_Api.src.Services;
using FrameworkDriver_Api.src.Utils;
using FrameworkDriver_Api.src.Utils.Interfaces;
using FrameworkDriver_Api.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure DataContext with connection strings from appsettings.json
builder.Services.Configure<DataContext>(
    builder.Configuration.GetSection("ConnectionStrings") ?? throw new Exception("Error al cargar la base de datos desde la configuracion"));

// iniciacion de servicios externos
builder.Services.AddScoped<Context>();

// add services for Services
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RegisterService>();
builder.Services.AddScoped<ObservationService>();

//  add services for repositories
builder.Services.AddScoped<IToken<UserModel>, Token>();

builder.Services.AddScoped<ICrud<ClientModel>, ClientRepository>();
builder.Services.AddScoped<ICrudWithLoad<UserModel>, UserRepository>();
builder.Services.AddScoped<ICrud<RegisterModel>, RegisterRepository>();
builder.Services.AddScoped<ICrud<ObservationModel>, ObservationRepository>();
builder.Services.AddScoped<ISession<SessionModel>, SessionRepository>();
//add utils
builder.Services.AddScoped<FileUpload>();

builder.Services.AddScoped<WhatsappInterface, WhatsappUtility>();

//add cloudinary key on settings
builder.Services.Configure<CloudinaryModel>(
    builder.Configuration.GetSection("Cloudinary") ?? throw new Exception("Error al cargar cloudinary desde la configuracion"));

// add WhasappService token on settings
builder.Services.Configure<WhatsappModel>(
    builder.Configuration.GetSection("Whatsapp") ?? throw new Exception(" error al cargar whatsapp desde la configuracion"));



// add services for controllers
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
