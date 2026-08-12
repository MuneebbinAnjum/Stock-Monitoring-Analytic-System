using System;

namespace SMAS.API.Models
{
    public class InventoryTransaction : Entity
    {
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
        public int QuantityChange { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid? RelatedOrderId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}