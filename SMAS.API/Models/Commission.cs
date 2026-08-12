using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("commissions")]
    public class Commission : Entity
    {
        [Column("employee_id")]
        [Required]
        public Guid EmployeeId { get; set; }

        [Column("product_id")]
        [Required]
        public Guid ProductId { get; set; }

        [Column("commission_percentage")]
        [Required]
        [Range(0, 100)]
        public decimal CommissionPercentage { get; set; }

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation properties
        public Employee? Employee { get; set; }
        public Product? Product { get; set; }
    }
}
