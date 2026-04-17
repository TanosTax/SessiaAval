using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SessiaAval.Data;
using SessiaAval.Interfaces;
using SessiaAval.Models;

namespace SessiaAval.Services;

public class AuthService : IAuthService
{
    private readonly DbContextFactory dbContextFactory;

    public AuthService(DbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<User?> loginAsync(string email, string password)
    {
        using var dbContext = dbContextFactory.createDbContext();
        var user = await dbContext.users
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.email == email && u.passwordHash == password);

        return user;
    }

    public async Task<(bool success, string message, User? user)> registerAsync(
        string email, string password, string firstName, string lastName, string? phone = null)
    {
        using var dbContext = dbContextFactory.createDbContext();
        
        var existingUser = await dbContext.users.FirstOrDefaultAsync(u => u.email == email);
        if (existingUser != null)
            return (false, "Пользователь с таким email уже существует", null);

        var userRole = await dbContext.roles.FirstOrDefaultAsync(r => r.roleName == "Пользователь");
        if (userRole == null)
            return (false, "Роль пользователя не найдена в системе", null);

        var newUser = new User
        {
            roleId = userRole.roleId,
            email = email,
            passwordHash = password,
            firstName = firstName,
            lastName = lastName,
            phone = phone,
            balance = 0,
            registrationDate = DateTime.Now,
            lastModified = DateTime.Now
        };

        dbContext.users.Add(newUser);
        await dbContext.SaveChangesAsync();

        newUser.role = userRole;
        return (true, "Регистрация успешна", newUser);
    }
}
