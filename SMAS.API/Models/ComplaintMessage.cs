using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("complaint_messages")]
    public class ComplaintMessage : Entity
    {
        [Column("complaint_id")]
        [Required]
        public Guid ComplaintId { get; set; }

        [Column("sender_id")]
        public Guid? SenderId { get; set; }

        [Column("sender_type")]
        [StringLength(50)]
        public string SenderType { get; set; } = "Buyer"; // "Buyer" or "Employee"

        [Column("message")]
        [Required]
        public string Message { get; set; } = string.Empty;

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        public Complaint? Complaint { get; set; }
    }
}
