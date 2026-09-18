using Rbac.Models;
using Rbac.Repositories;
using Rbac.Utils;

namespace Rbac.Services;

public class AuthService
{
    private readonly UserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;

    public AuthService(UserRepository userRepository, PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
            return null;

        bool valid = _passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        return valid ? user : null;
    }

    public async Task<Guid> RegisterUserAsync(string email, string password)
    {
        var (hash, salt) = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsEmailVerified = false,
            IsActive = true
        };

        return await _userRepository.CreateAsync(user);
    }
}
