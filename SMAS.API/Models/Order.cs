using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("orders")]
    public class Order : Entity
    {
        [Column("order_number")]
        [Required]
        [StringLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Column("employee_id")]
        public Guid? EmployeeId { get; set; }

        [Column("order_type")]
        [Required]
        [StringLength(20)]
        public string OrderType { get; set; } = "Online"; // Online or Physical

        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column("status")]
        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Processing, Packed, Dispatched, Delivered, Received, Cancelled, Returned

        [Column("total_amount")]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Column("tax_amount")]
        [Range(0, double.MaxValue)]
        public decimal TaxAmount { get; set; } = 0;

        [Column("delivery_charges")]
        [Range(0, double.MaxValue)]
        public decimal DeliveryCharges { get; set; } = 0;

        [Column("discount_amount")]
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [Column("delivery_city")]
        [StringLength(50)]
        public string? DeliveryCity { get; set; }

        [Column("delivery_address")]
        public string? DeliveryAddress { get; set; }

        [Column("courier_ref")]
        [StringLength(100)]
        public string? CourierRef { get; set; }

        [Column("delivery_period")]
        [StringLength(100)]
        public string? DeliveryPeriod { get; set; }

        [Column("payment_method")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Cash on Delivery";

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("admin_notes")]
        public string? AdminNotes { get; set; }

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }
        public Employee? Employee { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Complaint>? Complaints { get; set; } = new List<Complaint>();
    }
}