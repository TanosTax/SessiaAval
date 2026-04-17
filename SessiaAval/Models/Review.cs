using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SessiaAval.Models;

[Table("reviews")]
public class Review
{
    [Key]
    [Column("review_id")]
    public int reviewId { get; set; }
    
    [Required]
    [Column("user_id")]
    public int userId { get; set; }
    
    [Column("service_id")]
    public int? serviceId { get; set; }
    
    [Column("master_id")]
    public int? masterId { get; set; }
    
    [Column("rating")]
    [Range(1, 5)]
    public int rating { get; set; }
    
    [Column("comment")]
    public string? comment { get; set; }
    
    [Column("review_date")]
    public DateTime reviewDate { get; set; }
    
    [Column("last_modified")]
    public DateTime lastModified { get; set; }
    
    // Navigation properties
    [ForeignKey("userId")]
    public User? user { get; set; }
    
    [ForeignKey("serviceId")]
    public Service? service { get; set; }
    
    [ForeignKey("masterId")]
    public Master? master { get; set; }
}
