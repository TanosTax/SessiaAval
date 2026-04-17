using System.Threading.Tasks;
using SessiaAval.Models;

namespace SessiaAval.Interfaces;

public interface IAuthService
{
    Task<User?> loginAsync(string email, string password);
    Task<(bool success, string message, User? user)> registerAsync(string email, string password, string firstName, string lastName, string? phone = null);
}
