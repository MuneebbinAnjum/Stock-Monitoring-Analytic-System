using FluentValidation;

namespace SMAS.API.DTOs
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public Guid? SupplierId { get; set; }
        public string? Description { get; set; }
        public string? BrandName { get; set; }
        public string? CompanyName { get; set; }
        public string? Model { get; set; }
        public string DeliveryPeriod { get; set; } = "3-5 business days";
        public string? WarrantyInfo { get; set; }
        public string? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Tags { get; set; }
        public decimal TaxPercentage { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class ProductUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public Guid? SupplierId { get; set; }
        public string? Description { get; set; }
        public string? BrandName { get; set; }
        public string? CompanyName { get; set; }
        public string? Model { get; set; }
        public string DeliveryPeriod { get; set; } = "3-5 business days";
        public string? WarrantyInfo { get; set; }
        public string? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Tags { get; set; }
        public decimal TaxPercentage { get; set; }
        public List<string>? ImageUrls { get; set; }
        public byte[]? RowVersion { get; set; }
    }

    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? BrandName { get; set; }
        public string? CompanyName { get; set; }
        public string? Model { get; set; }
        public string DeliveryPeriod { get; set; } = string.Empty;
        public string? WarrantyInfo { get; set; }
        public string? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Tags { get; set; }
        public decimal TaxPercentage { get; set; }
        public int ViewCount { get; set; }
        public List<ProductImageResponseDto> ProductImages { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[]? RowVersion { get; set; }
    }

    public class ProductImageResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.SKU).NotEmpty().MaximumLength(50);
            // CategoryId must be provided
            RuleFor(x => x.CategoryId).NotEqual(Guid.Empty).WithMessage("Category is required");
            RuleFor(x => x.UnitPrice).GreaterThan(0);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        }
    }

    public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
    {
        public ProductUpdateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.SKU).NotEmpty().MaximumLength(50);
            // CategoryId must be provided (not Guid.Empty)
            RuleFor(x => x.CategoryId).NotEqual(Guid.Empty).WithMessage("Category is required");
            RuleFor(x => x.UnitPrice).GreaterThan(0);
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ReorderLevel).GreaterThanOrEqualTo(0);
        }
    }
}