using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("notifications")]
    public class Notification : Entity
    {
        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [Column("customer_id")]
        public Guid? CustomerId { get; set; }

        [Column("title")]
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("message")]
        [Required]
        public string Message { get; set; } = string.Empty;

        [Column("notification_type")]
        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty; 
        // NewOrder, NewComplaint, SalesmanSignup, LowStock, ReturnRequest, OrderDispatched, etc.

        [Column("related_id")]
        public Guid? RelatedId { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation property
        public Employee? Employee { get; set; }
        public Customer? Customer { get; set; }
    }
}
