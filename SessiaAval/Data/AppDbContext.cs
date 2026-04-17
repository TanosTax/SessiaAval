using Microsoft.EntityFrameworkCore;
using SessiaAval.Models;
using System;

namespace SessiaAval.Data;

public class AppDbContext : DbContext
{
    public DbSet<Role> roles { get; set; }
    public DbSet<User> users { get; set; }
    public DbSet<Collection> collections { get; set; }
    public DbSet<ServiceCategory> serviceCategories { get; set; }
    public DbSet<Master> masters { get; set; }
    public DbSet<Service> services { get; set; }
    public DbSet<MasterService> masterServices { get; set; }
    public DbSet<Appointment> appointments { get; set; }
    public DbSet<BalanceTransaction> balanceTransactions { get; set; }
    public DbSet<Review> reviews { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Настройка для работы с timestamp в PostgreSQL
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Настройка Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.roleId);
            entity.Property(e => e.roleId).ValueGeneratedOnAdd();
        });

        // Настройка User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.userId);
            entity.Property(e => e.userId).ValueGeneratedOnAdd();
            entity.Property(e => e.balance).HasPrecision(10, 2);
            entity.Property(e => e.registrationDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.lastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.role)
                .WithMany()
                .HasForeignKey(e => e.roleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Настройка Collection
        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.collectionId);
            entity.Property(e => e.collectionId).ValueGeneratedOnAdd();
            entity.Property(e => e.lastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Настройка ServiceCategory
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.categoryId);
            entity.Property(e => e.categoryId).ValueGeneratedOnAdd();
            entity.Property(e => e.lastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Настройка Master
        modelBuilder.Entity<Master>(entity =>
        {
            entity.HasKey(e => e.masterId);
            entity.Property(e => e.masterId).ValueGeneratedOnAdd();
            entity.Property(e => e.lastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.user)
                .WithMany()
                .HasForeignKey(e => e.userId)
                .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление мастера при удалении пользователя
        });

        // Настройка Service
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.serviceId);
            entity.Property(e => e.serviceId).ValueGeneratedOnAdd();
            entity.Property(e => e.price).HasPrecision(10, 2);
            entity.Property(e => e.createdDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.lastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.category)
                .WithMany()
                .HasForeignKey(e => e.categoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.collection)
                .WithMany()
                .HasForeignKey(e => e.collectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Настройка MasterService
        modelBuilder.Entity<MasterService>(entity =>
        {
            entity.HasKey(e => e.masterServiceId);
            entity.Property(e => e.masterServiceId).ValueGeneratedOnAdd();
            entity.Property(e => e.assignedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.lastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.master)
                .WithMany()
                .HasForeignKey(e => e.masterId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.service)
                .WithMany()
                .HasForeignKey(e => e.serviceId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => new { e.masterId, e.serviceId }).IsUnique();
        });

        // Настройка Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.appointmentId);
            entity.Property(e => e.appointmentId).ValueGeneratedOnAdd();
            entity.Property(e => e.createdDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.lastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.user)
                .WithMany()
                .HasForeignKey(e => e.userId)
                .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление записей при удалении пользователя
                
            entity.HasOne(e => e.master)
                .WithMany()
                .HasForeignKey(e => e.masterId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.service)
                .WithMany()
                .HasForeignKey(e => e.serviceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Настройка BalanceTransaction
        modelBuilder.Entity<BalanceTransaction>(entity =>
        {
            entity.HasKey(e => e.transactionId);
            entity.Property(e => e.transactionId).ValueGeneratedOnAdd();
            entity.Property(e => e.amount).HasPrecision(10, 2);
            entity.Property(e => e.transactionDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.user)
                .WithMany()
                .HasForeignKey(e => e.userId)
                .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление транзакций при удалении пользователя
        });

        // Настройка Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.reviewId);
            entity.Property(e => e.reviewId).ValueGeneratedOnAdd();
            entity.Property(e => e.reviewDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.lastModified).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(e => e.user)
                .WithMany()
                .HasForeignKey(e => e.userId)
                .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление отзывов при удалении пользователя
                
            entity.HasOne(e => e.service)
                .WithMany()
                .HasForeignKey(e => e.serviceId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.master)
                .WithMany()
                .HasForeignKey(e => e.masterId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
