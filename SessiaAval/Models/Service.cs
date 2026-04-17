using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace SessiaAval.Models;

[Table("services")]
public class Service
{
    [Key]
    [Column("service_id")]
    public int serviceId { get; set; }
    
    [Required]
    [Column("service_name")]
    [MaxLength(200)]
    public string serviceName { get; set; } = string.Empty;
    
    [Column("description")]
    public string? description { get; set; }
    
    [Required]
    [Column("price")]
    public decimal price { get; set; }
    
    [Required]
    [Column("duration_minutes")]
    public int durationMinutes { get; set; }
    
    [Required]
    [Column("category_id")]
    public int categoryId { get; set; }
    
    [Column("collection_id")]
    public int? collectionId { get; set; }
    
    [Column("is_active")]
    public bool isActive { get; set; } = true;
    
    [Column("created_date")]
    public DateTime createdDate { get; set; }
    
    [Column("last_modified")]
    public DateTime lastModified { get; set; }
    
    [NotMapped]
    public string imagePath
    {
        get
        {
            try
            {
                var categoryName = category?.categoryName?.ToLower() ?? "";
                
                if (categoryName.Contains("кастом") || categoryName.Contains("аксессуар") || 
                    categoryName.Contains("оружие") || categoryName.Contains("реквизит") || 
                    categoryName.Contains("парик") || categoryName.Contains("ремонт") || 
                    categoryName.Contains("аренда"))
                {
                    var index = ((serviceId - 1) % 12) + 1;
                    var path = $"avares://SessiaAval/Assets/Custom/Pr{index}.jpg";
                    Console.WriteLine($"Service {serviceId} ({serviceName}): Category={categoryName}, Image={path}");
                    return path;
                }
                else
                {
                    var index = ((serviceId - 1) % 7) + 1;
                    var path = $"avares://SessiaAval/Assets/Cosplay/KL{index}.jpg";
                    Console.WriteLine($"Service {serviceId} ({serviceName}): Category={categoryName}, Image={path}");
                    return path;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting image path: {ex.Message}");
                return "avares://SessiaAval/Assets/Logo.png";
            }
        }
    }
    
    [NotMapped]
    public Bitmap? imageSource
    {
        get
        {
            try
            {
                var uri = new Uri(imagePath);
                var assets = AssetLoader.Open(uri);
                return new Bitmap(assets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
                try
                {
                    var fallbackUri = new Uri("avares://SessiaAval/Assets/Logo.png");
                    var fallbackAssets = AssetLoader.Open(fallbackUri);
                    return new Bitmap(fallbackAssets);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
    
    // Navigation properties
    [ForeignKey("categoryId")]
    public ServiceCategory? category { get; set; }
    
    [ForeignKey("collectionId")]
    public Collection? collection { get; set; }
}

[Table("service_categories")]
public class ServiceCategory
{
    [Key]
    [Column("category_id")]
    public int categoryId { get; set; }
    
    [Required]
    [Column("category_name")]
    [MaxLength(100)]
    public string categoryName { get; set; } = string.Empty;
    
    [Column("description")]
    public string? description { get; set; }
    
    [Column("last_modified")]
    public DateTime lastModified { get; set; }
}

[Table("collections")]
public class Collection
{
    [Key]
    [Column("collection_id")]
    public int collectionId { get; set; }
    
    [Required]
    [Column("collection_name")]
    [MaxLength(100)]
    public string collectionName { get; set; } = string.Empty;
    
    [Column("description")]
    public string? description { get; set; }
    
    [Column("last_modified")]
    public DateTime lastModified { get; set; }
}
