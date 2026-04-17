using System;

namespace SessiaAval.Models;

[Flags]
public enum Permission
{
    None = 0,
    
    // Пользователь
    ViewBalance = 1 << 0,
    TopUpBalance = 1 << 1,
    BookAppointment = 1 << 2,
    WriteReview = 1 << 3,
    ViewOwnAppointments = 1 << 4,
    
    // Модератор
    ManageServices = 1 << 5,
    AssignServiceToMaster = 1 << 6,
    UnassignServiceFromMaster = 1 << 7,
    UpgradeMasterQualification = 1 << 8,
    ViewAllServices = 1 << 9,
    
    // Администратор
    ManageUsers = 1 << 10,
    ManageEmployees = 1 << 11,
    ViewAllUsers = 1 << 12,
    
    // Мастер
    ViewAssignedClients = 1 << 13,
    ViewAssignedServices = 1 << 14,
    RequestQualificationUpgrade = 1 << 15,
    
    // Общие
    ViewServices = 1 << 16,
    ViewCollections = 1 << 17
}
