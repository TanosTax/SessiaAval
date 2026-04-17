using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using SessiaAval.Data;
using SessiaAval.Models;

namespace SessiaAval.Tests.Integration;

[TestFixture]
public class ServiceManagementTests
{
    private AppDbContext _dbContext = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        SeedTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    private void SeedTestData()
    {
        var category1 = new ServiceCategory 
        { 
            categoryId = 1, 
            categoryName = "Стрижки",
            lastModified = DateTime.UtcNow
        };
        
        var category2 = new ServiceCategory 
        { 
            categoryId = 2, 
            categoryName = "Окрашивание",
            lastModified = DateTime.UtcNow
        };

        _dbContext.serviceCategories.AddRange(category1, category2);

        var masterRole = new Role { roleId = 2, roleName = "Мастер" };
        _dbContext.roles.Add(masterRole);

        var masterUser = new User
        {
            userId = 1,
            roleId = 2,
            email = "master@test.com",
            passwordHash = "123",
            firstName = "Петр",
            lastName = "Петров",
            balance = 0,
            registrationDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow
        };
        _dbContext.users.Add(masterUser);

        var master = new Master
        {
            masterId = 1,
            userId = 1,
            qualificationLevel = 1,
            hireDate = DateTime.UtcNow,
            qualificationRequestPending = false,
            lastModified = DateTime.UtcNow
        };
        _dbContext.masters.Add(master);

        _dbContext.SaveChanges();
    }

    [Test]
    public async Task ServiceLifecycle_CreateUpdateDelete_Success()
    {
        // Arrange - Создание услуги
        var category = await _dbContext.serviceCategories.FirstAsync();
        
        var service = new Service
        {
            serviceId = 1,
            categoryId = category.categoryId,
            serviceName = "Мужская стрижка",
            description = "Классическая стрижка",
            price = 500,
            durationMinutes = 30,
            createdDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow
        };

        // Act 1 - Создание
        _dbContext.services.Add(service);
        await _dbContext.SaveChangesAsync();

        // Assert 1 - Услуга создана
        var createdService = await _dbContext.services
            .Include(s => s.category)
            .FirstOrDefaultAsync(s => s.serviceId == service.serviceId);
        
        Assert.That(createdService, Is.Not.Null);
        Assert.That(createdService!.serviceName, Is.EqualTo("Мужская стрижка"));
        Assert.That(createdService.price, Is.EqualTo(500));
        Assert.That(createdService.category, Is.Not.Null);

        // Act 2 - Обновление
        createdService.price = 600;
        createdService.durationMinutes = 45;
        createdService.lastModified = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Assert 2 - Услуга обновлена
        var updatedService = await _dbContext.services.FindAsync(service.serviceId);
        Assert.That(updatedService!.price, Is.EqualTo(600));
        Assert.That(updatedService.durationMinutes, Is.EqualTo(45));

        // Act 3 - Удаление
        _dbContext.services.Remove(updatedService);
        await _dbContext.SaveChangesAsync();

        // Assert 3 - Услуга удалена
        var deletedService = await _dbContext.services.FindAsync(service.serviceId);
        Assert.That(deletedService, Is.Null);
    }

    [Test]
    public async Task MasterServiceAssignment_AssignAndUnassign_Success()
    {
        // Arrange
        var category = await _dbContext.serviceCategories.FirstAsync();
        var master = await _dbContext.masters.FirstAsync();

        var service = new Service
        {
            serviceId = 2,
            categoryId = category.categoryId,
            serviceName = "Женская стрижка",
            description = "Модельная стрижка",
            price = 800,
            durationMinutes = 60,
            createdDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow
        };
        _dbContext.services.Add(service);
        await _dbContext.SaveChangesAsync();

        // Act 1 - Назначение услуги мастеру
        var masterService = new MasterService
        {
            masterServiceId = 1,
            masterId = master.masterId,
            serviceId = service.serviceId
        };
        _dbContext.masterServices.Add(masterService);
        await _dbContext.SaveChangesAsync();

        // Assert 1 - Услуга назначена
        var assignedServices = await _dbContext.masterServices
            .Include(ms => ms.service)
            .Include(ms => ms.master)
            .Where(ms => ms.masterId == master.masterId)
            .ToListAsync();

        Assert.That(assignedServices.Count, Is.EqualTo(1));
        Assert.That(assignedServices[0].service!.serviceName, Is.EqualTo("Женская стрижка"));

        // Act 2 - Отмена назначения
        _dbContext.masterServices.Remove(masterService);
        await _dbContext.SaveChangesAsync();

        // Assert 2 - Назначение удалено
        var remainingServices = await _dbContext.masterServices
            .Where(ms => ms.masterId == master.masterId)
            .ToListAsync();

        Assert.That(remainingServices.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ServicesByCategory_FilteringWorks_Success()
    {
        // Arrange - Создаем услуги в разных категориях
        var category1 = await _dbContext.serviceCategories.FirstAsync(c => c.categoryName == "Стрижки");
        var category2 = await _dbContext.serviceCategories.FirstAsync(c => c.categoryName == "Окрашивание");

        var services = new[]
        {
            new Service
            {
                serviceId = 3,
                categoryId = category1.categoryId,
                serviceName = "Стрижка 1",
                price = 500,
                durationMinutes = 30,
                createdDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            },
            new Service
            {
                serviceId = 4,
                categoryId = category1.categoryId,
                serviceName = "Стрижка 2",
                price = 600,
                durationMinutes = 40,
                createdDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            },
            new Service
            {
                serviceId = 5,
                categoryId = category2.categoryId,
                serviceName = "Окрашивание 1",
                price = 2000,
                durationMinutes = 120,
                createdDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            }
        };

        _dbContext.services.AddRange(services);
        await _dbContext.SaveChangesAsync();

        // Act - Фильтрация по категории
        var haircutServices = await _dbContext.services
            .Where(s => s.categoryId == category1.categoryId)
            .ToListAsync();

        var coloringServices = await _dbContext.services
            .Where(s => s.categoryId == category2.categoryId)
            .ToListAsync();

        // Assert
        Assert.That(haircutServices.Count, Is.EqualTo(2));
        Assert.That(coloringServices.Count, Is.EqualTo(1));
        Assert.That(haircutServices.All(s => s.categoryId == category1.categoryId), Is.True);
        Assert.That(coloringServices.All(s => s.categoryId == category2.categoryId), Is.True);
    }

    [Test]
    public async Task ServicePriceRange_FilteringByPrice_Success()
    {
        // Arrange
        var category = await _dbContext.serviceCategories.FirstAsync();

        var services = new[]
        {
            new Service { serviceId = 6, categoryId = category.categoryId, serviceName = "Дешевая", price = 300, durationMinutes = 20, createdDate = DateTime.UtcNow, lastModified = DateTime.UtcNow },
            new Service { serviceId = 7, categoryId = category.categoryId, serviceName = "Средняя", price = 800, durationMinutes = 40, createdDate = DateTime.UtcNow, lastModified = DateTime.UtcNow },
            new Service { serviceId = 8, categoryId = category.categoryId, serviceName = "Дорогая", price = 2500, durationMinutes = 90, createdDate = DateTime.UtcNow, lastModified = DateTime.UtcNow }
        };

        _dbContext.services.AddRange(services);
        await _dbContext.SaveChangesAsync();

        // Act - Фильтрация по цене
        var affordableServices = await _dbContext.services
            .Where(s => s.price <= 1000)
            .OrderBy(s => s.price)
            .ToListAsync();

        var premiumServices = await _dbContext.services
            .Where(s => s.price > 1000)
            .ToListAsync();

        // Assert
        Assert.That(affordableServices.Count, Is.EqualTo(2));
        Assert.That(premiumServices.Count, Is.EqualTo(1));
        Assert.That(affordableServices[0].price, Is.LessThanOrEqualTo(affordableServices[1].price));
        Assert.That(premiumServices[0].price, Is.GreaterThan(1000));
    }

    [Test]
    public async Task MasterWithMultipleServices_CanHandleMultipleAssignments_Success()
    {
        // Arrange
        var category = await _dbContext.serviceCategories.FirstAsync();
        var master = await _dbContext.masters.FirstAsync();

        // Создаем несколько услуг
        for (int i = 1; i <= 5; i++)
        {
            var service = new Service
            {
                serviceId = 10 + i,
                categoryId = category.categoryId,
                serviceName = $"Услуга {i}",
                price = 500 * i,
                durationMinutes = 30 * i,
                createdDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            };
            _dbContext.services.Add(service);

            var masterService = new MasterService
            {
                masterServiceId = 10 + i,
                masterId = master.masterId,
                serviceId = service.serviceId
            };
            _dbContext.masterServices.Add(masterService);
        }

        await _dbContext.SaveChangesAsync();

        // Act - Получаем все услуги мастера
        var masterServices = await _dbContext.masterServices
            .Include(ms => ms.service)
            .Where(ms => ms.masterId == master.masterId)
            .ToListAsync();

        // Assert
        Assert.That(masterServices.Count, Is.EqualTo(5));
        Assert.That(masterServices.All(ms => ms.service != null), Is.True);
        
        var totalDuration = masterServices.Sum(ms => ms.service!.durationMinutes);
        Assert.That(totalDuration, Is.EqualTo(30 + 60 + 90 + 120 + 150)); // 450 минут
    }
}
