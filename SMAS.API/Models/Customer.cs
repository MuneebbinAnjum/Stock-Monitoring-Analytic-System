using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("customers")]
    public class Customer : Entity
    {
        [Column("full_name")]
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Column("email")]
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Column("city")]
        [StringLength(50)]
        public string? City { get; set; }

        [Column("province")]
        [StringLength(50)]
        public string? Province { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("postal_code")]
        [StringLength(10)]
        public string? PostalCode { get; set; }

        [Column("password_hash")]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation properties
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
        public ICollection<Complaint>? Complaints { get; set; } = new List<Complaint>();
    }
}