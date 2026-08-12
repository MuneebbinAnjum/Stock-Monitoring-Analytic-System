using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("system_settings")]
    public class SystemSetting : Entity
    {
        [Column("setting_key")]
        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Column("setting_value")]
        [Required]
        public string Value { get; set; } = string.Empty;

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("category")]
        [StringLength(50)]
        public string Category { get; set; } = "General"; // General, Tax, Delivery, Display

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }
    }
}
