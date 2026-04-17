using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SessiaAval.Interfaces;
using SessiaAval.Models;

namespace SessiaAval.Controllers;

public class ServiceController
{
    private readonly IServiceService serviceService;

    public ServiceController(IServiceService serviceService)
    {
        this.serviceService = serviceService;
    }

    public async Task<List<Service>> getServicesAsync(int page, int pageSize, string? sortBy = null, string? category = null, int? collectionId = null)
    {
        return await serviceService.getServicesAsync(page, pageSize, sortBy, category, collectionId);
    }

    public async Task<int> getTotalCountAsync(string? category = null, int? collectionId = null)
    {
        return await serviceService.getTotalCountAsync(category, collectionId);
    }

    public async Task<Service> addServiceAsync(Service service)
    {
        return await serviceService.addServiceAsync(service);
    }

    public async Task<bool> updateServiceAsync(Service service)
    {
        return await serviceService.updateServiceAsync(service);
    }

    public async Task<bool> deleteServiceAsync(int serviceId)
    {
        return await serviceService.deleteServiceAsync(serviceId);
    }

    public async Task<DateTime?> getLastModifiedAsync(int serviceId)
    {
        return await serviceService.getLastModifiedAsync(serviceId);
    }

    public async Task<List<ServiceCategory>> getCategoriesAsync()
    {
        return await serviceService.getCategoriesAsync();
    }

    public async Task<List<Collection>> getCollectionsAsync()
    {
        return await serviceService.getCollectionsAsync();
    }
}
