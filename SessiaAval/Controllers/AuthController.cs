using System.Threading.Tasks;
using SessiaAval.Interfaces;
using SessiaAval.Models;

namespace SessiaAval.Controllers;

public class AuthController
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    public async Task<User?> loginAsync(string email, string password)
    {
        return await authService.loginAsync(email, password);
    }

    public async Task<(bool success, string message, User? user)> registerAsync(
        string email, string password, string firstName, string lastName, string? phone = null)
    {
        return await authService.registerAsync(email, password, firstName, lastName, phone);
    }
}
