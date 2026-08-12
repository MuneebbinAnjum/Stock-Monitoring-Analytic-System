using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("product_images")]
    public class ProductImage : Entity
    {
        [Column("product_id")]
        public Guid ProductId { get; set; }

        [Column("image_url")]
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("alt_text")]
        [StringLength(200)]
        public string? AltText { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation property
        public Product? Product { get; set; }
    }
}
