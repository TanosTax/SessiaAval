using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SessiaAval.Models;

[Table("balance_transactions")]
public class BalanceTransaction
{
    [Key]
    [Column("transaction_id")]
    public int transactionId { get; set; }
    
    [Required]
    [Column("user_id")]
    public int userId { get; set; }
    
    [Required]
    [Column("amount")]
    public decimal amount { get; set; }
    
    [Required]
    [Column("transaction_type")]
    [MaxLength(20)]
    public string transactionType { get; set; } = string.Empty;
    
    [Column("card_last_digits")]
    [MaxLength(4)]
    public string? cardLastDigits { get; set; }
    
    [Column("transaction_date")]
    public DateTime transactionDate { get; set; }
    
    // Navigation property
    [ForeignKey("userId")]
    public User? user { get; set; }
}
