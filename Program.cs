using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Rbac.Middleware;
using Rbac.Repositories;
using Rbac.Services;
using Rbac.Utils;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Configuration
// ------------------------------------------------------------
var config = builder.Configuration;

// ------------------------------------------------------------
// Dependency Injection
// ------------------------------------------------------------
builder.Services.AddSingleton<SqlConnectionFactory>();

// Repositories
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<RoleRepository>();
builder.Services.AddSingleton<RbacQueryRepository>();

// Utilities
builder.Services.AddSingleton<PasswordHasher>();

// Services
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<RbacQueryService>();
builder.Services.AddSingleton<JwtIssuer>();

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// ------------------------------------------------------------
// JWT Authentication (RS256)
// ------------------------------------------------------------
var publicKeyPath = config["Jwt:PublicKeyPath"];
if (string.IsNullOrWhiteSpace(publicKeyPath))
    throw new InvalidOperationException("Jwt:PublicKeyPath is missing in configuration.");

var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(publicKeyPath));

var rsaSecurityKey = new RsaSecurityKey(rsa);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false; // dev-friendly
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = config["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = config["Jwt:Audience"],

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = rsaSecurityKey,

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------
builder.Services.AddAuthorization();

// ------------------------------------------------------------
// Build App
// ------------------------------------------------------------
var app = builder.Build();

// 
app.Use(async (context, next) =>
{
    Console.WriteLine("Pipeline: " + context.Request.Path);
    Console.WriteLine("Auth: " + context.User.Identity?.IsAuthenticated);
    await next();
});

// ------------------------------------------------------------
// Middleware Pipeline
// ------------------------------------------------------------
app.UseRouting();
app.UseCors("AllowUI");


app.UseAuthentication();
app.UseAuthorization();

// Custom RBAC permission middleware (runs AFTER auth)
app.UseMiddleware<PermissionAuthorizationMiddleware>();

app.MapControllers();

app.Run();
