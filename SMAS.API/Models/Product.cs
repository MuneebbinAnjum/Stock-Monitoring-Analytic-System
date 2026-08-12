using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace SMAS.API.Models
{
    [Table("products")]
public class Product : Entity
{
        [Column("name")]
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Column("sku")]
        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Column("barcode")]
        [StringLength(50)]
        public string? Barcode { get; set; }

        [Column("category_id")]
        public Guid CategoryId { get; set; }

        [Column("supplier_id")]
        public Guid? SupplierId { get; set; }

        [Column("unit_price")]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Column("purchase_price")]
        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        [Column("discount_price")]
        [Range(0, double.MaxValue)]
        public decimal? DiscountPrice { get; set; }

        [Column("tax_percentage")]
        [Range(0, 100)]
        public decimal TaxPercentage { get; set; } = 0;

        [Column("stock_quantity")]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Column("reorder_level")]
        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("brand_name")]
        [StringLength(100)]
        public string? BrandName { get; set; }

        [Column("company_name")]
        [StringLength(100)]
        public string? CompanyName { get; set; }

        [Column("model")]
        [StringLength(100)]
        public string? Model { get; set; }

        [Column("delivery_period")]
        [StringLength(100)]
        public string DeliveryPeriod { get; set; } = "3-5 business days";

        [Column("warranty_info")]
        [StringLength(200)]
        public string? WarrantyInfo { get; set; }

        [Column("weight")]
        [StringLength(50)]
        public string? Weight { get; set; }

        [Column("dimensions")]
        [StringLength(100)]
        public string? Dimensions { get; set; }

        [Column("tags")]
        [StringLength(500)]
        public string? Tags { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Draft, Archived

        [Column("is_featured")]
        public bool IsFeatured { get; set; } = false;

        [Column("view_count")]
        public int ViewCount { get; set; } = 0;

        [Column("created_at")]
        public override DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public override DateTime UpdatedAt { get; set; }

        [Column("is_deleted")]
        public override bool IsDeleted { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<SaleRecord>? SaleRecords { get; set; } = new List<SaleRecord>();
        public ICollection<ProductImage>? ProductImages { get; set; } = new List<ProductImage>();

        [Timestamp]
        [Column("row_version")]
        public byte[]? RowVersion { get; set; }
    }
}