using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SessiaAval.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("user_id")]
    public int userId { get; set; }
    
    [Required]
    [Column("role_id")]
    public int roleId { get; set; }
    
    [Required]
    [Column("email")]
    [MaxLength(100)]
    public string email { get; set; } = string.Empty;
    
    [Required]
    [Column("password_hash")]
    [MaxLength(255)]
    public string passwordHash { get; set; } = string.Empty;
    
    [Required]
    [Column("first_name")]
    [MaxLength(50)]
    public string firstName { get; set; } = string.Empty;
    
    [Required]
    [Column("last_name")]
    [MaxLength(50)]
    public string lastName { get; set; } = string.Empty;
    
    [Column("phone")]
    [MaxLength(20)]
    public string? phone { get; set; }
    
    [Column("balance")]
    public decimal balance { get; set; } = 0;
    
    [Column("registration_date")]
    public DateTime registrationDate { get; set; }
    
    [Column("last_modified")]
    public DateTime lastModified { get; set; }
    
    // Navigation property
    [ForeignKey("roleId")]
    public Role? role { get; set; }
    
    public string fullName => $"{firstName} {lastName}";
}
