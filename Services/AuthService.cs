using System.Security.Cryptography;
using System.Text;
using JournalAppBlazor.Models;
using JournalAppBlazor.Repositories;

namespace JournalAppBlazor.Services;

public interface IAuthService
{
    Task<bool> HasAnyUsersAsync();
    Task<(bool Success, string Error)> RegisterAsync(string username, string password);
    Task<(bool Success, string Error)> LoginAsync(string username, string password);
    Task LogoutAsync();
    bool IsAuthenticated { get; }
    string? CurrentUsername { get; }
    int? CurrentUserId { get; }
}

public class AuthService : IAuthService
{
    private readonly IUserRepositoryFactory _userRepository;
    private const string IsAuthenticatedKey = "journal_app_authenticated";
    private const string CurrentUserIdKey = "journal_app_user_id";
    private const string CurrentUsernameKey = "journal_app_username";

    public bool IsAuthenticated { get; private set; }
    public string? CurrentUsername { get; private set; }
    public int? CurrentUserId { get; private set; }

    public AuthService(IUserRepositoryFactory userRepository)
    {
        _userRepository = userRepository;
        
        // Restore session state
        IsAuthenticated = Preferences.Get(IsAuthenticatedKey, false);
        CurrentUserId = Preferences.Get(CurrentUserIdKey, 0);
        CurrentUsername = Preferences.Get(CurrentUsernameKey, string.Empty);
        
        if (CurrentUserId == 0)
        {
            CurrentUserId = null;
            CurrentUsername = null;
            IsAuthenticated = false;
        }
    }

    public async Task<bool> HasAnyUsersAsync()
    {
        return await _userRepository.HasAnyUsersAsync();
    }

    public async Task<(bool Success, string Error)> RegisterAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Username is required.");

        if (username.Length < 3)
            return (false, "Username must be at least 3 characters.");

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required.");

        if (password.Length < 4)
            return (false, "Password must be at least 4 characters.");

        // Check if username already exists
        var existingUser = await _userRepository.GetByUsernameAsync(username);

        if (existingUser != null)
            return (false, "Username already taken.");

        // Create new user
        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateUserAsync(user);

        // Auto-login after registration
        SetAuthenticatedState(user);

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return (false, "Username is required.");

        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required.");

        var user = await _userRepository.GetByUsernameAsync(username);

        if (user == null)
            return (false, "Invalid username or password.");

        if (!VerifyPassword(password, user.PasswordHash))
            return (false, "Invalid username or password.");

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        SetAuthenticatedState(user);

        return (true, string.Empty);
    }

    public Task LogoutAsync()
    {
        IsAuthenticated = false;
        CurrentUserId = null;
        CurrentUsername = null;

        Preferences.Set(IsAuthenticatedKey, false);
        Preferences.Remove(CurrentUserIdKey);
        Preferences.Remove(CurrentUsernameKey);

        return Task.CompletedTask;
    }

    private void SetAuthenticatedState(User user)
    {
        IsAuthenticated = true;
        CurrentUserId = user.Id;
        CurrentUsername = user.Username;

        Preferences.Set(IsAuthenticatedKey, true);
        Preferences.Set(CurrentUserIdKey, user.Id);
        Preferences.Set(CurrentUsernameKey, user.Username);
    }

    private string HashPassword(string password)
    {
        // Generate a salt
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Use SHA256 for hashing
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt));
            var hash = sha256.ComputeHash(saltedPassword);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
            return false;

        var salt = parts[0];
        var hash = parts[1];

        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = Encoding.UTF8.GetBytes(password + salt);
            var computedHash = sha256.ComputeHash(saltedPassword);
            var computedHashString = Convert.ToBase64String(computedHash);
            return hash == computedHashString;
        }
    }
}
