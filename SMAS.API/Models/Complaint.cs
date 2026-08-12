using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("complaints")]
    public class Complaint : Entity
    {
        [Column("order_id")]
        public Guid OrderId { get; set; }

        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Column("complaint_type")]
        [Required]
        [StringLength(50)]
        public string ComplaintType { get; set; } = "Product"; // Product, Delivery, Return Request

        [Column("title")]
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        [Required]
        public string Description { get; set; } = string.Empty;

        [Column("status")]
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open"; // Open, In Review, Resolved, Rejected

        [Column("admin_notes")]
        public string? AdminNotes { get; set; }

        [Column("return_approved")]
        public bool? ReturnApproved { get; set; }

        [Column("evidence_image_url")]
        [StringLength(500)]
        public string? EvidenceImageUrl { get; set; }

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("resolved_at")]
        public DateTime? ResolvedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation properties
        public Order? Order { get; set; }
        public Customer? Customer { get; set; }
        public ICollection<ComplaintMessage>? Messages { get; set; } = new List<ComplaintMessage>();
    }
}
