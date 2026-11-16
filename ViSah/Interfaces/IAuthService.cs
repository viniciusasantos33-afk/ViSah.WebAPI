using ViSah.Services;

namespace ViSah.Interfaces;

public interface IAuthService
{
    Task<string?> Register(RegisterRequest r);
    Task<(string? Token, string? Error)> Login(LoginRequest r);
}