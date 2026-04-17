using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SessiaAval.Models;

[Table("master_services")]
public class MasterService
{
    [Key]
    [Column("master_service_id")]
    public int masterServiceId { get; set; }
    
    [Required]
    [Column("master_id")]
    public int masterId { get; set; }
    
    [Required]
    [Column("service_id")]
    public int serviceId { get; set; }
    
    [Column("assigned_date")]
    public DateTime assignedDate { get; set; }
    
    [Column("last_modified")]
    public DateTime lastModified { get; set; }
    
    // Navigation properties
    [ForeignKey("masterId")]
    public Master? master { get; set; }
    
    [ForeignKey("serviceId")]
    public Service? service { get; set; }
}
