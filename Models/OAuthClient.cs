namespace Rbac.Models;

public class OAuthClient
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public byte[]? ClientSecretHash { get; set; }
    public string? AllowedScopes { get; set; }
    public string? RedirectUris { get; set; }
    public DateTime CreatedAt { get; set; }
}
