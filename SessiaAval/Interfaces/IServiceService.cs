using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SessiaAval.Models;

namespace SessiaAval.Interfaces;

public interface IServiceService
{
    Task<List<Service>> getServicesAsync(int page, int pageSize, string? sortBy = null, string? category = null, int? collectionId = null);
    Task<int> getTotalCountAsync(string? category = null, int? collectionId = null);
    Task<Service> addServiceAsync(Service service);
    Task<bool> updateServiceAsync(Service service);
    Task<bool> deleteServiceAsync(int serviceId);
    Task<DateTime?> getLastModifiedAsync(int serviceId);
    Task<List<ServiceCategory>> getCategoriesAsync();
    Task<List<Collection>> getCollectionsAsync();
}
