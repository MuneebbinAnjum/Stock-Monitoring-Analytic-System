using FluentValidation;

namespace SMAS.API.DTOs
{
    public class OrderCreateDto
    {
        public Guid CustomerId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string OrderType { get; set; } = "Online";
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash on Delivery";
        public string DeliveryPeriod { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderUpdateDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string DeliveryPeriod { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string CourierRef { get; set; } = string.Empty;
        public List<OrderItemResponseDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
    {
        public OrderCreateDtoValidator()
        {
            // CustomerId is supplied by the backend for authenticated buyers.
            RuleFor(x => x.Items).NotEmpty().Must(items => items.Count > 0);
            RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());
            RuleFor(x => x.DeliveryCity).NotEmpty().WithMessage("Delivery city is required.");
            RuleFor(x => x.DeliveryAddress).NotEmpty().WithMessage("Delivery address is required.");
            RuleFor(x => x.PaymentMethod).NotEmpty();
        }
    }

    public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
    {
        public OrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(1000); // Prevent unreasonable quantities
        }
    }

    public class OrderUpdateDtoValidator : AbstractValidator<OrderUpdateDto>
    {
        public OrderUpdateDtoValidator()
        {
            RuleFor(x => x.Status).NotEmpty().Must(s => new[] { "Pending", "Approved", "Rejected", "Processing", "Dispatched", "Delivered", "Cancelled" }.Contains(s));
        }
    }
}