namespace SMAS.API.DTOs
{
    // ── Complaint DTOs ──
    public class ComplaintCreateDto
    {
        public string? OrderNumber { get; set; }
        public string ComplaintType { get; set; } = "Product";
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? EvidenceImageUrl { get; set; }
    }

    public class ComplaintStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public string? ComplaintType { get; set; }
        public bool? ReturnApproved { get; set; }
    }

    public class ComplaintResponseDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ComplaintType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public bool? ReturnApproved { get; set; }
        public string? EvidenceImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ComplaintMessageDto> Messages { get; set; } = new();
    }

    public class ComplaintMessageDto
    {
        public Guid Id { get; set; }
        public Guid ComplaintId { get; set; }
        public Guid? SenderId { get; set; }
        public string SenderType { get; set; } = string.Empty; // Buyer or Employee
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateComplaintMessageDto
    {
        public string Message { get; set; } = string.Empty;
    }

    // ── Notification DTOs ──
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public Guid? RelatedId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Cart DTOs ──
    public class CartItemCreateDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class CartItemUpdateDto
    {
        public int Quantity { get; set; }
    }

    public class CartItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public int StockAvailable { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxPercentage { get; set; }
    }

    // ── Wishlist DTOs ──
    public class WishlistItemCreateDto
    {
        public Guid ProductId { get; set; }
    }

    public class WishlistItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public bool InStock { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Settings DTOs ──
    public class SystemSettingDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class SystemSettingUpdateDto
    {
        public string Value { get; set; } = string.Empty;
    }

    // ── AuditLog DTOs ──
    public class AuditLogResponseDto
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime PerformedAt { get; set; }
        public string Details { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
    }
}
