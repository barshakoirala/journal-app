using System.Security.Cryptography;
using System.Text;

namespace JournalAppBlazor.Services;

public interface IAuthService
{
    Task<bool> IsPasswordSetAsync();
    Task<bool> SetPasswordAsync(string password);
    Task<bool> VerifyPasswordAsync(string password);
    Task<bool> ChangePasswordAsync(string oldPassword, string newPassword);
    bool IsAuthenticated { get; }
    Task LoginAsync(string password);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private const string PasswordHashKey = "journal_app_password_hash";
    private const string IsAuthenticatedKey = "journal_app_authenticated";
    
    public bool IsAuthenticated { get; private set; }

    public AuthService()
    {
        // Check if already authenticated in this session
        IsAuthenticated = Preferences.Get(IsAuthenticatedKey, false);
    }

    public async Task<bool> IsPasswordSetAsync()
    {
        var hash = Preferences.Get(PasswordHashKey, string.Empty);
        return !string.IsNullOrEmpty(hash);
    }

    public async Task<bool> SetPasswordAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        var hash = HashPassword(password);
        Preferences.Set(PasswordHashKey, hash);
        IsAuthenticated = true;
        Preferences.Set(IsAuthenticatedKey, true);
        return true;
    }

    public async Task<bool> VerifyPasswordAsync(string password)
    {
        var storedHash = Preferences.Get(PasswordHashKey, string.Empty);
        if (string.IsNullOrEmpty(storedHash))
            return false;

        return VerifyPasswordHash(password, storedHash);
    }

    public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        if (!await VerifyPasswordAsync(oldPassword))
            return false;

        return await SetPasswordAsync(newPassword);
    }

    public async Task LoginAsync(string password)
    {
        if (await VerifyPasswordAsync(password))
        {
            IsAuthenticated = true;
            Preferences.Set(IsAuthenticatedKey, true);
        }
        else
        {
            throw new UnauthorizedAccessException("Invalid password");
        }
    }

    public async Task LogoutAsync()
    {
        IsAuthenticated = false;
        Preferences.Set(IsAuthenticatedKey, false);
        await Task.CompletedTask;
    }

    private string HashPassword(string password)
    {
        // Generate a salt
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Use SHA256 for hashing (simpler than PBKDF2 for this use case)
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt));
            var hash = sha256.ComputeHash(saltedPassword);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }
    }

    private bool VerifyPasswordHash(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = parts[1];

        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt));
            var computedHash = sha256.ComputeHash(saltedPassword);
            var computedHashString = Convert.ToBase64String(computedHash);
            return hash == computedHashString;
        }
    }
}
