using System;
using Microsoft.EntityFrameworkCore;

namespace SessiaAval.Data;

public static class DbInitializer
{
    public static void initialize(AppDbContext context)
    {
        try
        {
            // Проверить подключение к БД
            context.Database.CanConnect();
            Console.WriteLine("✅ Подключение к БД успешно");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка подключения к БД: {ex.Message}");
            Console.WriteLine("\nПроверьте:");
            Console.WriteLine("1. PostgreSQL запущен");
            Console.WriteLine("2. База данных matye_db создана");
            Console.WriteLine("3. Пользователь matye_user существует");
            Console.WriteLine("4. Строка подключения корректна");
            Console.WriteLine("\nДля создания БД выполните:");
            Console.WriteLine("  cd Database");
            Console.WriteLine("  psql -U matye_user -d matye_db -f schema.sql");
            Console.WriteLine("  psql -U matye_user -d matye_db -f test_data.sql");
            throw;
        }
    }

    public static AppDbContext createDbContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        
        var context = new AppDbContext(optionsBuilder.Options);
        initialize(context);
        
        return context;
    }
}
