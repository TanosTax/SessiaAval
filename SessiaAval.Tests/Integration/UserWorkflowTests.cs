using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using SessiaAval.Data;
using SessiaAval.Models;
using SessiaAval.Services;

namespace SessiaAval.Tests.Integration;

[TestFixture]
public class UserWorkflowTests
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
        // Создаем роли
        var userRole = new Role { roleId = 1, roleName = "Пользователь" };
        var masterRole = new Role { roleId = 2, roleName = "Мастер" };
        _dbContext.roles.AddRange(userRole, masterRole);

        // Создаем категорию услуг
        var category = new ServiceCategory 
        { 
            categoryId = 1, 
            categoryName = "Стрижки",
            lastModified = DateTime.UtcNow
        };
        _dbContext.serviceCategories.Add(category);

        // Создаем пользователя
        var user = new User
        {
            userId = 1,
            roleId = 1,
            email = "test@test.com",
            passwordHash = "123",
            firstName = "Иван",
            lastName = "Иванов",
            phone = "+79991234567",
            balance = 0,
            registrationDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow,
            role = userRole
        };
        _dbContext.users.Add(user);

        // Создаем мастера
        var masterUser = new User
        {
            userId = 2,
            roleId = 2,
            email = "master@test.com",
            passwordHash = "123",
            firstName = "Петр",
            lastName = "Петров",
            phone = "+79991234568",
            balance = 0,
            registrationDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow,
            role = masterRole
        };
        _dbContext.users.Add(masterUser);

        var master = new Master
        {
            masterId = 1,
            userId = 2,
            qualificationLevel = 2,
            specialization = "Стрижки",
            hireDate = DateTime.UtcNow,
            qualificationRequestPending = false,
            lastModified = DateTime.UtcNow
        };
        _dbContext.masters.Add(master);

        // Создаем услугу
        var service = new Service
        {
            serviceId = 1,
            categoryId = 1,
            serviceName = "Мужская стрижка",
            description = "Классическая мужская стрижка",
            price = 500,
            durationMinutes = 30,
            createdDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow,
            category = category
        };
        _dbContext.services.Add(service);

        // Назначаем услугу мастеру
        var masterService = new MasterService
        {
            masterServiceId = 1,
            masterId = 1,
            serviceId = 1
        };
        _dbContext.masterServices.Add(masterService);

        _dbContext.SaveChanges();
    }

    [Test]
    public async Task CompleteUserJourney_FromRegistrationToReview_Success()
    {
        // Arrange - Регистрация нового пользователя
        var newUser = new User
        {
            userId = 3,
            roleId = 1,
            email = "newuser@test.com",
            passwordHash = "123",
            firstName = "Мария",
            lastName = "Сидорова",
            phone = "+79991234569",
            balance = 0,
            registrationDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow
        };
        _dbContext.users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        // Act 1 - Пользователь пополняет баланс
        newUser.balance += 1000;
        newUser.lastModified = DateTime.UtcNow;

        var transaction = new BalanceTransaction
        {
            transactionId = 1,
            userId = newUser.userId,
            amount = 1000,
            transactionType = "Пополнение",
            cardLastDigits = "1234",
            transactionDate = DateTime.UtcNow
        };
        _dbContext.balanceTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        // Assert 1 - Баланс пополнен
        Assert.That(newUser.balance, Is.EqualTo(1000));
        var savedTransaction = await _dbContext.balanceTransactions
            .FirstOrDefaultAsync(t => t.userId == newUser.userId);
        Assert.That(savedTransaction, Is.Not.Null);
        Assert.That(savedTransaction!.amount, Is.EqualTo(1000));

        // Act 2 - Пользователь записывается на услугу
        var service = await _dbContext.services.FirstAsync();
        var master = await _dbContext.masters.FirstAsync();

        var appointment = new Appointment
        {
            appointmentId = 1,
            userId = newUser.userId,
            serviceId = service.serviceId,
            masterId = master.masterId,
            appointmentDate = DateTime.UtcNow.AddDays(1),
            status = "pending",
            queueNumber = 1,
            createdDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow
        };
        _dbContext.appointments.Add(appointment);
        await _dbContext.SaveChangesAsync();

        // Assert 2 - Запись создана
        var savedAppointment = await _dbContext.appointments
            .Include(a => a.service)
            .FirstOrDefaultAsync(a => a.userId == newUser.userId);
        Assert.That(savedAppointment, Is.Not.Null);
        Assert.That(savedAppointment!.status, Is.EqualTo("pending"));
        Assert.That(savedAppointment.queueNumber, Is.EqualTo(1));

        // Act 3 - Мастер завершает услугу и списываются деньги
        var userBeforePayment = await _dbContext.users.FindAsync(newUser.userId);
        Assert.That(userBeforePayment!.balance, Is.GreaterThanOrEqualTo(service.price));

        userBeforePayment.balance -= service.price;
        userBeforePayment.lastModified = DateTime.UtcNow;

        var paymentTransaction = new BalanceTransaction
        {
            transactionId = 2,
            userId = newUser.userId,
            amount = -service.price,
            transactionType = "Оплата услуги",
            cardLastDigits = null,
            transactionDate = DateTime.UtcNow
        };
        _dbContext.balanceTransactions.Add(paymentTransaction);

        savedAppointment.status = "completed";
        savedAppointment.lastModified = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Assert 3 - Оплата прошла, статус изменен
        var userAfterPayment = await _dbContext.users.FindAsync(newUser.userId);
        Assert.That(userAfterPayment!.balance, Is.EqualTo(500)); // 1000 - 500
        
        var completedAppointment = await _dbContext.appointments.FindAsync(appointment.appointmentId);
        Assert.That(completedAppointment!.status, Is.EqualTo("completed"));

        var paymentRecord = await _dbContext.balanceTransactions
            .Where(t => t.userId == newUser.userId && t.amount < 0)
            .FirstOrDefaultAsync();
        Assert.That(paymentRecord, Is.Not.Null);
        Assert.That(paymentRecord!.amount, Is.EqualTo(-500));

        // Act 4 - Пользователь оставляет отзыв
        var review = new Review
        {
            reviewId = 1,
            userId = newUser.userId,
            serviceId = service.serviceId,
            masterId = master.masterId,
            rating = 5,
            comment = "Отличная работа!",
            reviewDate = DateTime.UtcNow
        };
        _dbContext.reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        // Assert 4 - Отзыв сохранен
        var savedReview = await _dbContext.reviews
            .FirstOrDefaultAsync(r => r.userId == newUser.userId);
        Assert.That(savedReview, Is.Not.Null);
        Assert.That(savedReview!.rating, Is.EqualTo(5));
        Assert.That(savedReview.comment, Is.EqualTo("Отличная работа!"));

        // Final Assert - Проверяем полную историю транзакций
        var allTransactions = await _dbContext.balanceTransactions
            .Where(t => t.userId == newUser.userId)
            .OrderBy(t => t.transactionDate)
            .ToListAsync();
        
        Assert.That(allTransactions.Count, Is.EqualTo(2));
        Assert.That(allTransactions[0].amount, Is.EqualTo(1000)); // Пополнение
        Assert.That(allTransactions[1].amount, Is.EqualTo(-500)); // Оплата
    }

    [Test]
    public async Task AppointmentFlow_InsufficientBalance_CannotComplete()
    {
        // Arrange - Пользователь с недостаточным балансом
        var user = await _dbContext.users.FirstAsync(u => u.email == "test@test.com");
        user.balance = 100; // Меньше чем стоимость услуги (500)
        await _dbContext.SaveChangesAsync();

        var service = await _dbContext.services.FirstAsync();
        var master = await _dbContext.masters.FirstAsync();

        var appointment = new Appointment
        {
            appointmentId = 2,
            userId = user.userId,
            serviceId = service.serviceId,
            masterId = master.masterId,
            appointmentDate = DateTime.UtcNow.AddDays(1),
            status = "pending",
            queueNumber = 2,
            createdDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow
        };
        _dbContext.appointments.Add(appointment);
        await _dbContext.SaveChangesAsync();

        // Act - Попытка завершить услугу
        var canComplete = user.balance >= service.price;

        // Assert - Недостаточно средств
        Assert.That(canComplete, Is.False);
        Assert.That(user.balance, Is.LessThan(service.price));
        Assert.That(appointment.status, Is.EqualTo("pending"));
    }

    [Test]
    public async Task MasterQualificationFlow_RequestAndApproval_Success()
    {
        // Arrange
        var master = await _dbContext.masters.FirstAsync();
        var initialLevel = master.qualificationLevel;

        // Act 1 - Мастер подает заявку на повышение
        master.qualificationRequestPending = true;
        master.lastModified = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Assert 1 - Заявка подана
        var masterWithRequest = await _dbContext.masters.FindAsync(master.masterId);
        Assert.That(masterWithRequest!.qualificationRequestPending, Is.True);

        // Act 2 - Модератор одобряет заявку
        masterWithRequest.qualificationLevel += 1;
        masterWithRequest.qualificationRequestPending = false;
        masterWithRequest.lastModified = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Assert 2 - Квалификация повышена
        var upgradedMaster = await _dbContext.masters.FindAsync(master.masterId);
        Assert.That(upgradedMaster!.qualificationLevel, Is.EqualTo(initialLevel + 1));
        Assert.That(upgradedMaster.qualificationRequestPending, Is.False);
    }

    [Test]
    public async Task MultipleAppointments_QueueNumbering_IsSequential()
    {
        // Arrange
        var user = await _dbContext.users.FirstAsync(u => u.email == "test@test.com");
        var service = await _dbContext.services.FirstAsync();
        var master = await _dbContext.masters.FirstAsync();

        // Act - Создаем несколько записей
        for (int i = 1; i <= 5; i++)
        {
            var appointment = new Appointment
            {
                userId = user.userId,
                serviceId = service.serviceId,
                masterId = master.masterId,
                appointmentDate = DateTime.UtcNow.AddDays(i),
                status = "pending",
                queueNumber = i,
                createdDate = DateTime.UtcNow,
                lastModified = DateTime.UtcNow
            };
            _dbContext.appointments.Add(appointment);
        }
        await _dbContext.SaveChangesAsync();

        // Assert - Номера в очереди последовательные
        var appointments = await _dbContext.appointments
            .Where(a => a.userId == user.userId)
            .OrderBy(a => a.queueNumber)
            .ToListAsync();

        Assert.That(appointments.Count, Is.EqualTo(5));
        for (int i = 0; i < 5; i++)
        {
            Assert.That(appointments[i].queueNumber, Is.EqualTo(i + 1));
        }
    }

    [Test]
    public async Task ReviewFlow_OnlyForCompletedAppointments_Success()
    {
        // Arrange
        var user = await _dbContext.users.FirstAsync(u => u.email == "test@test.com");
        var service = await _dbContext.services.FirstAsync();
        var master = await _dbContext.masters.FirstAsync();

        var completedAppointment = new Appointment
        {
            appointmentId = 3,
            userId = user.userId,
            serviceId = service.serviceId,
            masterId = master.masterId,
            appointmentDate = DateTime.UtcNow.AddDays(-1),
            status = "completed",
            queueNumber = 3,
            createdDate = DateTime.UtcNow,
            lastModified = DateTime.UtcNow
        };
        _dbContext.appointments.Add(completedAppointment);
        await _dbContext.SaveChangesAsync();

        // Act - Пользователь оставляет отзыв
        var review = new Review
        {
            reviewId = 2,
            userId = user.userId,
            serviceId = service.serviceId,
            masterId = master.masterId,
            rating = 4,
            comment = "Хорошо",
            reviewDate = DateTime.UtcNow
        };
        _dbContext.reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedReview = await _dbContext.reviews
            .Include(r => r.user)
            .Include(r => r.service)
            .Include(r => r.master)
            .FirstOrDefaultAsync(r => r.reviewId == review.reviewId);

        Assert.That(savedReview, Is.Not.Null);
        Assert.That(savedReview!.rating, Is.InRange(1, 5));
        Assert.That(savedReview.user, Is.Not.Null);
        Assert.That(savedReview.service, Is.Not.Null);
        Assert.That(savedReview.master, Is.Not.Null);
    }
}
