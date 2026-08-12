using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("categories")]
    public class Category : Entity
    {
        [Column("name")]
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("image_url")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Column("parent_category_id")]
        public Guid? ParentCategoryId { get; set; }

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation properties
        public Category? ParentCategory { get; set; }
        public ICollection<Category>? SubCategories { get; set; } = new List<Category>();
    }
}