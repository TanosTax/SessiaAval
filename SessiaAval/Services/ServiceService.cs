using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SessiaAval.Data;
using SessiaAval.Interfaces;
using SessiaAval.Models;

namespace SessiaAval.Services;

public class ServiceService : IServiceService
{
    private readonly DbContextFactory dbContextFactory;

    public ServiceService(DbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<List<Service>> getServicesAsync(int page, int pageSize, string? sortBy = null, string? category = null, int? collectionId = null)
    {
        using var dbContext = dbContextFactory.createDbContext();
        var query = dbContext.services
            .Include(s => s.category)
            .Include(s => s.collection)
            .Where(s => s.isActive);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(s => s.category != null && s.category.categoryName == category);
        }

        if (collectionId.HasValue && collectionId.Value > 0)
        {
            query = query.Where(s => s.collectionId == collectionId.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "name" => query.OrderBy(s => s.serviceName),
            "price" => query.OrderBy(s => s.price),
            "date" => query.OrderByDescending(s => s.lastModified),
            _ => query.OrderBy(s => s.serviceName)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> getTotalCountAsync(string? category = null, int? collectionId = null)
    {
        using var dbContext = dbContextFactory.createDbContext();
        var query = dbContext.services.Where(s => s.isActive);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(s => s.category != null && s.category.categoryName == category);
        }

        if (collectionId.HasValue && collectionId.Value > 0)
        {
            query = query.Where(s => s.collectionId == collectionId.Value);
        }

        return await query.CountAsync();
    }

    public async Task<Service> addServiceAsync(Service service)
    {
        using var dbContext = dbContextFactory.createDbContext();
        service.createdDate = DateTime.Now;
        service.lastModified = DateTime.Now;
        service.isActive = true;

        dbContext.services.Add(service);
        await dbContext.SaveChangesAsync();
        
        return service;
    }

    public async Task<bool> updateServiceAsync(Service service)
    {
        using var dbContext = dbContextFactory.createDbContext();
        var existing = await dbContext.services.FindAsync(service.serviceId);
        if (existing == null)
            return false;

        existing.serviceName = service.serviceName;
        existing.description = service.description;
        existing.price = service.price;
        existing.durationMinutes = service.durationMinutes;
        existing.categoryId = service.categoryId;
        existing.collectionId = service.collectionId;
        existing.lastModified = DateTime.Now;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> deleteServiceAsync(int serviceId)
    {
        using var dbContext = dbContextFactory.createDbContext();
        var service = await dbContext.services.FindAsync(serviceId);
        if (service == null)
            return false;

        service.isActive = false;
        service.lastModified = DateTime.Now;
        
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<DateTime?> getLastModifiedAsync(int serviceId)
    {
        using var dbContext = dbContextFactory.createDbContext();
        var service = await dbContext.services.FindAsync(serviceId);
        return service?.lastModified;
    }

    public async Task<List<ServiceCategory>> getCategoriesAsync()
    {
        using var dbContext = dbContextFactory.createDbContext();
        return await dbContext.serviceCategories.ToListAsync();
    }

    public async Task<List<Collection>> getCollectionsAsync()
    {
        using var dbContext = dbContextFactory.createDbContext();
        return await dbContext.collections.ToListAsync();
    }
}
