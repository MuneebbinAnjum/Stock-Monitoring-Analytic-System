using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("audit_logs")]
    public class AuditLog : Entity
    {
        [Column("entity_name")]
        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [Column("entity_id")]
        public Guid? EntityId { get; set; }

        [Column("action")]
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Column("performed_by")]
        [Required]
        [StringLength(200)]
        public string PerformedBy { get; set; } = string.Empty;

        [Column("performed_at")]
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        [Column("details")]
        public string Details { get; set; } = string.Empty;

        [Column("old_values")]
        public string? OldValues { get; set; }

        [Column("new_values")]
        public string? NewValues { get; set; }

        [Column("ip_address")]
        [StringLength(50)]
        public string? IpAddress { get; set; }
    }
}
