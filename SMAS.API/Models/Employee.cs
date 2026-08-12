using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("employees")]
    public class Employee : Entity
    {
        [Column("full_name")]
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Column("role")]
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Salesman"; // Admin or Salesman

        [Column("email")]
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("hire_date")]
        public DateTime HireDate { get; set; }

        [Column("monthly_sales_target")]
        [Precision(18, 2)]
        public decimal MonthlySalesTarget { get; set; }

        [Column("monthly_salary")]
        [Precision(18, 2)]
        public decimal MonthlySalary { get; set; } = 0;

        [Column("approval_status")]
        [StringLength(20)]
        public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation properties
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
        public ICollection<SaleRecord>? SaleRecords { get; set; } = new List<SaleRecord>();
        public ICollection<Notification>? Notifications { get; set; } = new List<Notification>();
    }
}