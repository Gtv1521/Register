using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FrameworkDriver_Api.Models;
using FrameworkDriver_Api.src.Dto;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Projections;
using FrameworkDriver_Api.src.Repositories;
using FrameworkDriver_Api.src.Services;
using FrameworkDriver_Api.src.Utils;
using FrameworkDriver_Api.src.Utils.Interfaces;
using FrameworkDriver_Api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MongoDB.Driver;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure DataContext with connection strings from appsettings.json
builder.Services.Configure<DataContext>(
    builder.Configuration.GetSection("ConnectionStrings") ?? throw new Exception("Error al cargar la base de datos desde la configuracion"));

//add cloudinary key on settings
builder.Services.Configure<CloudinaryModel>(
    builder.Configuration.GetSection("Cloudinary") ?? throw new Exception("Error al cargar cloudinary desde la configuracion"));

// add WhasappService token on settings
builder.Services.Configure<WhatsappModel>(
    builder.Configuration.GetSection("Whatsapp") ?? throw new Exception("Error al cargar whatsapp desde la configuracion"));

// add email Key services
builder.Services.Configure<EmailModel>(
    builder.Configuration.GetSection("Email") ?? throw new Exception("Error de server email"));

// ppciones para swagger  
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var blacklist = context.HttpContext.RequestServices.GetRequiredService<IToken<UserModel>>();
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (jti != null && blacklist.IsRevoked(jti)) context.Fail("Token revocado");
                return Task.CompletedTask;
            },

            OnMessageReceived = context =>
            {
                // Busca la cookie llamada "access_token"
                if (context.Request.Cookies.ContainsKey("access_token")) context.Token = context.Request.Cookies["access_token"];
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                context.HandleResponse(); // Evitar el manejo predeterminado
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                var response = new { Message = "No autorizado. Token invalido o expirado." };
                var jsonResponse = JsonSerializer.Serialize(response);
                return context.Response.WriteAsync(jsonResponse);
            },
        };
    });


builder.Services.AddAuthorization();

// iniciacion de servicios externos
builder.Services.AddScoped<Context>();

// add services for Services
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RegisterService>();
builder.Services.AddScoped<ObservationService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<RegisterService>();
builder.Services.AddScoped<ObservationService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<IHashPass<UserDto>, HashPassword>();
//  add services for repositories
builder.Services.AddScoped<IToken<UserModel>, Token>();

// add repositories
builder.Services.AddScoped<IAddFilter<ClientModel, ClientModel>, ClientRepository>();
builder.Services.AddScoped<ICrudWithLoad<UserModel>, UserRepository>();
builder.Services.AddScoped<IRegisters<RegisterModel, ListRegistersProjection, RegisterObsCliProjection>, RegisterRepository>();
builder.Services.AddScoped<ILoadAllId<ObservationModel>, ObservationRepository>();
builder.Services.AddScoped<ISession<SessionModel>, SessionRepository>();
builder.Services.AddScoped<QrInterface, QrService>();
//add utils
builder.Services.AddScoped<FileUpload>();

builder.Services.AddScoped<WhatsappInterface, WhatsappUtility>();

// add services for controllers
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();

        // Usa IOpenApiSecurityScheme en lugar de OpenApiSecurityScheme
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["BearerAuth"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header usando Bearer scheme. Ejemplo: Bearer eyJhbGciOi..."
        };

        return Task.CompletedTask;
    });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors("AllowAngular");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Register-Api")
           .WithClassicLayout()
           .ForceDarkMode()
           .HideSearch()
           .ShowOperationId()
           .ExpandAllTags()
           .SortTagsAlphabetically()
           .SortOperationsByMethod()
           .AddPreferredSecuritySchemes("BearerAuth")
           .PreserveSchemaPropertyOrder();
        //    .WithProxy("https://api-gateway.company.com")
        //    .AddServer("https://api.company.com", "Production")
        //    .AddServer("https://staging-api.company.com", "Staging");
    });

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
