using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("notification_reads")]
    public class NotificationRead : Entity
    {
        [Column("notification_id")]
        [Required]
        public Guid NotificationId { get; set; }

        [Column("user_id")]
        [Required]
        public Guid UserId { get; set; }

        [Column("user_type")]
        [StringLength(50)]
        public string UserType { get; set; } = "Employee"; // "Employee" or "Buyer"

        [Column("is_read")]
        public bool IsRead { get; set; } = true;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        public Notification? Notification { get; set; }
    }
}
