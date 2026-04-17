using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SessiaAval.Models;

[Table("roles")]
public class Role
{
    [Key]
    [Column("role_id")]
    public int roleId { get; set; }
    
    [Required]
    [Column("role_name")]
    [MaxLength(50)]
    public string roleName { get; set; } = string.Empty;
    
    [Column("description")]
    public string? description { get; set; }
}
