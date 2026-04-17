using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SessiaAval.Models;

[Table("masters")]
public class Master
{
    [Key]
    [Column("master_id")]
    public int masterId { get; set; }
    
    [Required]
    [Column("user_id")]
    public int userId { get; set; }
    
    [Column("qualification_level")]
    public int qualificationLevel { get; set; } = 1;
    
    [Column("specialization")]
    public string? specialization { get; set; }
    
    [Required]
    [Column("hire_date")]
    public DateTime hireDate { get; set; }
    
    [Column("qualification_request_pending")]
    public bool qualificationRequestPending { get; set; } = false;
    
    [Column("last_modified")]
    public DateTime lastModified { get; set; }
    
    // Navigation property
    [ForeignKey("userId")]
    public User? user { get; set; }
}
