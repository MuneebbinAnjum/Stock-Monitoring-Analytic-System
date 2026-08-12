using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMAS.API.Models
{
    [Table("cart_items")]
    public class CartItem : Entity
    {
        [Column("customer_id")]
        public Guid CustomerId { get; set; }

        [Column("product_id")]
        public Guid ProductId { get; set; }

        [Column("quantity")]
        [Range(1, 10000)]
        public int Quantity { get; set; } = 1;

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }
        public Product? Product { get; set; }
    }
}
