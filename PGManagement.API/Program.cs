using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PGManagement.API.Data;
using PGManagement.API.Helpers;
using PGManagement.API.Services;
using Swashbuckle.AspNetCore.Swagger;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMachineStatusService, MachineStatusService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? builder.Configuration["JwtSettings:Key"]
    ?? builder.Configuration["Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? builder.Configuration["JwtSettings:Issuer"]
    ?? builder.Configuration["Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? builder.Configuration["JwtSettings:Audience"]
    ?? builder.Configuration["Audience"];

if (!string.IsNullOrWhiteSpace(jwtKey) && !string.IsNullOrWhiteSpace(jwtIssuer) && !string.IsNullOrWhiteSpace(jwtAudience))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
}

var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(defaultConnectionString);
if (sqlConnectionStringBuilder.ConnectTimeout < 30)
{
    sqlConnectionStringBuilder.ConnectTimeout = 30;
}

builder.Services.AddDbContext<PGManagementDbContext>(options =>
{
    options.UseSqlServer(sqlConnectionStringBuilder.ConnectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(30);
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .WithOrigins("http://localhost:4200");
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PGManagementDbContext>();
    dbContext.Database.Migrate();
}

if (FirebaseApp.DefaultInstance == null)
{
    var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];
    GoogleCredential credential;

    if (!string.IsNullOrWhiteSpace(firebaseCredentialsPath))
    {
        if (!File.Exists(firebaseCredentialsPath))
        {
            throw new InvalidOperationException($"Firebase credentials file not found at '{firebaseCredentialsPath}'.");
        }

        credential = GoogleCredential.FromFile(firebaseCredentialsPath);
    }
    else
    {
        credential = GoogleCredential.GetApplicationDefault();
    }

    FirebaseApp.Create(new AppOptions
    {
        Credential = credential
    });
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseMiddleware<FirebaseAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Hello World");

app.Run();






