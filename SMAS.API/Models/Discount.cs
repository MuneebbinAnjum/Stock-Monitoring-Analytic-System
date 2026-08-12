using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("discounts")]
    public class Discount : Entity
    {
        [Column("product_id")]
        [Required]
        public Guid ProductId { get; set; }

        [Column("discount_percentage")]
        [Required]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        [Column("start_date")]
        [Required]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        [Required]
        public DateTime EndDate { get; set; }

        [Column("created_by_admin")]
        [StringLength(100)]
        public string? CreatedByAdmin { get; set; }

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation property
        public Product? Product { get; set; }
    }
}
