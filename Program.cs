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
builder.Services.AddSingleton<PermissionRepository>();
builder.Services.AddSingleton<RbacQueryRepository>();

// Utilities
builder.Services.AddSingleton<PasswordHasher>();

// Services
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<RbacQueryService>();
builder.Services.AddSingleton<JwtIssuer>();

// Controllers
builder.Services.AddControllers();

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
.AddJwtBearer(options =>
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

// ------------------------------------------------------------
// Middleware Pipeline
// ------------------------------------------------------------
app.UseRouting();

app.UseAuthentication();

// Custom RBAC permission middleware
app.UseMiddleware<PermissionAuthorizationMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
