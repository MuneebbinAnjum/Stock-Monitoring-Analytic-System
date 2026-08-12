using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Services;

namespace SMAS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly SmasDbContext _context;

        public OrdersController(IOrderService orderService, SmasDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        // Admin/Manager only: returns all orders
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetOrdersWithDetailsAsync();
            return Ok(new ApiResponse<IEnumerable<OrderResponseDto>> { Success = true, Data = orders });
        }

        // Salesman-specific: only returns orders they created (physical orders)
        [Authorize(Roles = "Salesman")]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMySalesmanOrders()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (employee == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Employee not found" });

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.EmployeeId == employee.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var dtos = orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.FullName ?? string.Empty,
                EmployeeId = o.EmployeeId,
                EmployeeName = o.Employee?.FullName ?? string.Empty,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                TaxAmount = o.TaxAmount,
                DiscountAmount = o.DiscountAmount,
                DeliveryCity = o.DeliveryCity ?? string.Empty,
                DeliveryAddress = o.DeliveryAddress ?? string.Empty,
                DeliveryPeriod = o.DeliveryPeriod ?? string.Empty,
                PaymentMethod = o.PaymentMethod ?? string.Empty,
                CourierRef = o.CourierRef ?? string.Empty,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList(),
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            });

            return Ok(new ApiResponse<IEnumerable<OrderResponseDto>> { Success = true, Data = dtos });
        }

        // Buyer-specific: only returns orders belonging to the authenticated buyer
        [Authorize(Roles = "Buyer")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse<string> { Success = false, Message = "Not authenticated" });

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Customer not found" });

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customer.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Map using the same DTO structure
            var dtos = orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.FullName ?? string.Empty,
                EmployeeId = o.EmployeeId,
                EmployeeName = o.Employee?.FullName ?? string.Empty,
                OrderDate = o.OrderDate,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                TaxAmount = o.TaxAmount,
                DiscountAmount = o.DiscountAmount,
                DeliveryCity = o.DeliveryCity ?? string.Empty,
                DeliveryAddress = o.DeliveryAddress ?? string.Empty,
                DeliveryPeriod = o.DeliveryPeriod ?? string.Empty,
                PaymentMethod = o.PaymentMethod ?? string.Empty,
                CourierRef = o.CourierRef ?? string.Empty,
                Items = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList(),
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            });

            return Ok(new ApiResponse<IEnumerable<OrderResponseDto>> { Success = true, Data = dtos });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(new ApiResponse<OrderResponseDto> { Success = true, Data = order });
        }

        [Authorize(Roles = "Admin,Manager,Salesman,Buyer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            // Secure override for Buyers
            if (User.IsInRole("Buyer"))
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(email))
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                    if (customer != null)
                    {
                        dto.CustomerId = customer.Id;
                    }
                }
            }
            else if (User.IsInRole("Salesman"))
            {
                // Salesmen can only create orders for customers assigned to them
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var salesman = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
                if (salesman == null)
                {
                    return Unauthorized(new ApiResponse<string> { Success = false, Message = "Salesman not found" });
                }
                // For now, allow salesman to create orders (implementation of salesman-customer assignment can be enhanced)
                // This prevents empty CustomerId but validates salesman exists
                if (dto.CustomerId == Guid.Empty)
                {
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "CustomerId is required" });
                }
                // Validate customer exists
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId && !c.IsDeleted);
                if (customer == null)
                {
                    return BadRequest(new ApiResponse<string> { Success = false, Message = "Customer not found or inactive" });
                }
            }
            else if (dto.CustomerId == Guid.Empty)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = "CustomerId is required for non-buyer orders." });
            }

            var order = await _orderService.CreateOrderAsync(dto);
            return Ok(new ApiResponse<OrderResponseDto> { Success = true, Data = order, Message = "Order created" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderUpdateDto dto)
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, dto.Status);
            return Ok(new ApiResponse<OrderResponseDto> { Success = true, Data = order, Message = "Order status updated" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id}/dispatch")]
        public async Task<IActionResult> Dispatch(Guid id, [FromBody] string courierType)
        {
            await _orderService.DispatchViaCourierAsync(id, courierType);
            return Ok(new ApiResponse<string> { Success = true, Message = "Order dispatched" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var order = await _orderService.CancelOrderAsync(id);
            return Ok(new ApiResponse<OrderResponseDto> { Success = true, Data = order, Message = "Order cancelled and stock restored" });
        }

        [Authorize(Roles = "Admin,Manager,Salesman,Buyer")]
        [HttpPost("{id}/receive")]
        public async Task<IActionResult> Receive(Guid id)
        {
            var order = await _orderService.ReceiveOrderAsync(id);
            return Ok(new ApiResponse<OrderResponseDto> { Success = true, Data = order, Message = "Order marked as received" });
        }

        [AllowAnonymous]
        [HttpGet("number/{orderNumber}")]
        public async Task<IActionResult> GetByNumber(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            return Ok(new ApiResponse<OrderResponseDto> { Success = true, Data = order });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var order = await _orderService.ApproveOrderAsync(id);
            return Ok(new ApiResponse<OrderResponseDto> { Success = true, Data = order, Message = "Order approved" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var order = await _orderService.RejectOrderAsync(id);
            return Ok(new ApiResponse<OrderResponseDto> { Success = true, Data = order, Message = "Order rejected" });
        }
    }
}