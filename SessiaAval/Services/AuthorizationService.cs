using System.Collections.Generic;
using SessiaAval.Models;

namespace SessiaAval.Services;

public class AuthorizationService
{
    private static readonly Dictionary<string, Permission> rolePermissions = new()
    {
        ["пользователь"] = 
            Permission.ViewBalance |
            Permission.TopUpBalance |
            Permission.BookAppointment |
            Permission.WriteReview |
            Permission.ViewOwnAppointments |
            Permission.ViewServices |
            Permission.ViewCollections,
            
        ["модератор"] = 
            Permission.ManageServices |
            Permission.AssignServiceToMaster |
            Permission.UnassignServiceFromMaster |
            Permission.UpgradeMasterQualification |
            Permission.ViewAllServices |
            Permission.ViewServices |
            Permission.ViewCollections,
            
        ["администратор"] = 
            Permission.ManageUsers |
            Permission.ManageEmployees |
            Permission.ViewAllUsers |
            Permission.ViewServices |
            Permission.ViewCollections,
            
        ["мастер"] = 
            Permission.ViewAssignedClients |
            Permission.ViewAssignedServices |
            Permission.RequestQualificationUpgrade |
            Permission.ViewServices |
            Permission.ViewCollections
    };
    
    public static bool hasPermission(User user, Permission permission)
    {
        if (user?.role?.roleName == null)
            return false;
            
        var roleName = user.role.roleName.ToLower();
        
        if (!rolePermissions.ContainsKey(roleName))
            return false;
            
        return (rolePermissions[roleName] & permission) == permission;
    }
    
    public static Permission getUserPermissions(User user)
    {
        if (user?.role?.roleName == null)
            return Permission.None;
            
        var roleName = user.role.roleName.ToLower();
        
        return rolePermissions.ContainsKey(roleName) 
            ? rolePermissions[roleName] 
            : Permission.None;
    }
    
    public static bool isUser(User user) => 
        user?.role?.roleName?.ToLower() == "пользователь";
    
    public static bool isModerator(User user) => 
        user?.role?.roleName?.ToLower() == "модератор";
    
    public static bool isAdmin(User user) => 
        user?.role?.roleName?.ToLower() == "администратор";
    
    public static bool isMaster(User user) => 
        user?.role?.roleName?.ToLower() == "мастер";
}
