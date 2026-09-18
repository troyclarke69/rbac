using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Rbac.Repositories;

namespace Rbac.Services;

public class JwtIssuer
{
    private readonly RbacQueryService _rbacService;
    private readonly IConfiguration _config;

    public JwtIssuer(RbacQueryService rbacService, IConfiguration config)
    {
        _rbacService = rbacService;
        _config = config;
    }

    public async Task<string> GenerateTokenAsync(Guid userId)
    {
        // Load configuration safely (no nullability warnings)
        var issuer = _config.GetValue<string>("Jwt:Issuer")!;
        var audience = _config.GetValue<string>("Jwt:Audience")!;
        var privateKeyPath = _config.GetValue<string>("Jwt:PrivateKeyPath")!;
        var lifetimeMinutes = _config.GetValue<int>("Jwt:TokenLifetimeMinutes");

        // Load RSA private key
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(privateKeyPath));

        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256
        );

        // Load RBAC data
        var roles = await _rbacService.GetUserRolesAsync(userId);
        var permissions = await _rbacService.GetUserPermissionsAsync(userId);

        // Build claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, issuer),
            new Claim(JwtRegisteredClaimNames.Aud, audience),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim("role", role));

        foreach (var perm in permissions)
            claims.Add(new Claim("permission", perm));

        // Build token
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(lifetimeMinutes),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
