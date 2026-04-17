using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SessiaAval.Models;

[Table("appointments")]
public class Appointment
{
    [Key]
    [Column("appointment_id")]
    public int appointmentId { get; set; }
    
    [Required]
    [Column("user_id")]
    public int userId { get; set; }
    
    [Required]
    [Column("master_id")]
    public int masterId { get; set; }
    
    [Required]
    [Column("service_id")]
    public int serviceId { get; set; }
    
    [Required]
    [Column("appointment_date")]
    public DateTime appointmentDate { get; set; }
    
    [Required]
    [Column("queue_number")]
    public int queueNumber { get; set; }
    
    [Column("status")]
    [MaxLength(20)]
    public string status { get; set; } = "pending";
    
    [Column("created_date")]
    public DateTime createdDate { get; set; }
    
    [Column("last_modified")]
    public DateTime lastModified { get; set; }
    
    // Navigation properties
    [ForeignKey("userId")]
    public User? user { get; set; }
    
    [ForeignKey("masterId")]
    public Master? master { get; set; }
    
    [ForeignKey("serviceId")]
    public Service? service { get; set; }
}
