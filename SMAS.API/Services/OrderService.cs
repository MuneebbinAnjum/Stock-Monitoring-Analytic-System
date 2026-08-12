using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Models;
using SMAS.API.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace SMAS.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly SmasDbContext _context;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<SMAS.API.Hubs.NotificationHub> _hubContext;

        public OrderService(IOrderRepository orderRepository, SmasDbContext context, Microsoft.AspNetCore.SignalR.IHubContext<SMAS.API.Hubs.NotificationHub> hubContext)
        {
            _orderRepository = orderRepository;
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await BeginTransactionIfSupportedAsync();
                try
                {
                    var customer = await _context.Customers.FindAsync(dto.CustomerId);
                    if (customer == null) throw new KeyNotFoundException("Customer not found");

                    if (dto.Items == null || dto.Items.Count == 0)
                        throw new InvalidOperationException("Order items are required");

                    // We'll compute subtotal before discount, total discount, tax and final totals
                    decimal subtotalBeforeDiscount = 0m;
                    decimal totalDiscount = 0m;
                    decimal totalTax = 0m;
                    var orderItems = new List<OrderItem>();

                    var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
                    var products = await _context.Products
                        .Where(p => productIds.Contains(p.Id))
                        .ToListAsync();

                    // Load any active discounts for these products
                    var now = DateTime.UtcNow;
                    var activeDiscounts = await _context.Discounts
                        .Where(d => productIds.Contains(d.ProductId) && d.StartDate <= now && d.EndDate >= now && !d.IsDeleted)
                        .ToListAsync();
                    var discountMap = activeDiscounts.ToDictionary(d => d.ProductId, d => d.DiscountPercentage);

                    foreach (var item in dto.Items)
                    {
                        if (item.Quantity <= 0) throw new InvalidOperationException("Quantity must be greater than zero");

                        var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (product == null) throw new KeyNotFoundException($"Product {item.ProductId} not found");
                        if (product.StockQuantity < item.Quantity) throw new InvalidOperationException($"Insufficient stock for {product.Name}");

                        var unitPrice = product.UnitPrice;
                        subtotalBeforeDiscount += unitPrice * item.Quantity;

                        // Determine effective price: prefer active Discount table entry, fallback to product.DiscountPrice
                        decimal effectivePrice = unitPrice;
                        if (discountMap.TryGetValue(product.Id, out var pct))
                        {
                            effectivePrice = unitPrice * (1 - (pct / 100m));
                            totalDiscount += (unitPrice - effectivePrice) * item.Quantity;
                        }
                        else if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
                        {
                            effectivePrice = product.DiscountPrice.Value;
                            totalDiscount += (unitPrice - effectivePrice) * item.Quantity;
                        }

                        totalTax += (effectivePrice * item.Quantity) * (product.TaxPercentage / 100);

                        orderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = effectivePrice
                        });

                        product.StockQuantity -= item.Quantity;

                        var invTx = new InventoryTransaction
                        {
                            ProductId = product.Id,
                            QuantityChange = -item.Quantity,
                            Reason = "Order creation",
                            CreatedBy = "system"
                        };
                        _context.InventoryTransactions.Add(invTx);

                        if (dto.EmployeeId.HasValue && dto.EmployeeId.Value != Guid.Empty)
                        {
                            var sale = new SaleRecord
                            {
                                ProductId = product.Id,
                                EmployeeId = dto.EmployeeId.Value,
                                SaleDate = DateTime.UtcNow,
                                QuantitySold = item.Quantity,
                                Revenue = effectivePrice * item.Quantity
                            };
                            _context.SaleRecords.Add(sale);
                        }
                    }

                    decimal deliveryCharge = subtotalBeforeDiscount > 5000 ? 0 : 250;
                    decimal finalTotalAmount = subtotalBeforeDiscount - totalDiscount + totalTax + deliveryCharge;

                    var order = new Order
                    {
                        CustomerId = dto.CustomerId,
                        EmployeeId = dto.EmployeeId,
                        OrderNumber = GenerateOrderNumber(),
                        OrderType = string.IsNullOrWhiteSpace(dto.OrderType) ? "Online" : dto.OrderType,
                        TotalAmount = finalTotalAmount,
                        TaxAmount = totalTax,
                        DiscountAmount = totalDiscount,
                        DeliveryCharges = deliveryCharge,
                        DeliveryCity = string.IsNullOrWhiteSpace(dto.DeliveryCity) ? customer.City : dto.DeliveryCity,
                        DeliveryAddress = dto.DeliveryAddress,
                        PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "Cash on Delivery" : dto.PaymentMethod,
                        DeliveryPeriod = string.IsNullOrWhiteSpace(dto.DeliveryPeriod) ? "Standard" : dto.DeliveryPeriod,
                        OrderItems = orderItems,
                        Status = dto.OrderType == "Physical" ? "Received" : "Pending"
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    if (transaction != null) await transaction.CommitAsync();

                    await CheckAndCreateAlertsAsync();

                    // Notify admin of new order (broadcast - EmployeeId null means admin sees it)
                    _context.Notifications.Add(new Notification
                    {
                        Title = "New Order Received",
                        Message = $"Order {order.OrderNumber} placed by {customer.FullName} for Rs. {order.TotalAmount:N0}. Awaiting approval.",
                        NotificationType = "NewOrder",
                        RelatedId = order.Id
                    });

                    // Audit log
                    _context.AuditLogs.Add(new AuditLog
                    {
                        EntityName = "Order",
                        EntityId = order.Id,
                        Action = "Create",
                        PerformedBy = customer.Email,
                        PerformedAt = DateTime.UtcNow,
                        Details = $"Order {order.OrderNumber} created, total Rs. {order.TotalAmount:N0}, {orderItems.Count} items"
                    });
                    await _context.SaveChangesAsync();

                    if (_hubContext != null)
                    {
                        try { await _hubContext.Clients.Group(order.CustomerId.ToString()).SendAsync("NewOrder", new { OrderId = order.Id, Total = order.TotalAmount }); } catch { }
                    }

                    return await MapToResponseDtoAsync(order);
                }
                catch
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            order.Status = status;
            var updated = await _orderRepository.UpdateAsync(order);
            return await MapToResponseDtoAsync(updated);
        }

        public async Task<OrderResponseDto> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null) throw new KeyNotFoundException("Order not found");
            return await MapToResponseDtoAsync(order);
        }

        public async Task<OrderResponseDto> ApproveOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            order.Status = "Approved";
            var updated = await _orderRepository.UpdateAsync(order, saveChanges: false);

            var notification = new Notification
            {
                Title = "Order Approved",
                Message = $"Your order {order.OrderNumber} has been approved and is being processed.",
                NotificationType = "OrderApproved",
                RelatedId = order.Id,
                CustomerId = order.CustomerId
            };
            _context.Notifications.Add(notification);

            if (_hubContext != null)
            {
                try { await _hubContext.Clients.Group(order.CustomerId.ToString()).SendAsync("OrderApproved", new { OrderId = order.Id }); } catch { }
            }

            // Audit log
            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Order",
                EntityId = order.Id,
                Action = "Approve",
                PerformedBy = "Admin",
                PerformedAt = DateTime.UtcNow,
                Details = $"Order {order.OrderNumber} approved"
            });

            await _context.SaveChangesAsync();
            return await MapToResponseDtoAsync(updated);
        }

        public async Task<OrderResponseDto> RejectOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            // Restore stock on rejection
            var orderWithItems = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (orderWithItems != null)
            {
                foreach (var item in orderWithItems.OrderItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductId = product.Id,
                            QuantityChange = item.Quantity,
                            Reason = "Order rejected - stock restored",
                            CreatedBy = "system"
                        });
                    }
                }
            }

            order.Status = "Rejected";
            var updated = await _orderRepository.UpdateAsync(order, saveChanges: false);

            _context.Notifications.Add(new Notification
            {
                Title = "Order Rejected",
                Message = $"Your order {order.OrderNumber} has been rejected. Stock has been restored.",
                NotificationType = "OrderRejected",
                RelatedId = order.Id,
                CustomerId = order.CustomerId
            });

            if (_hubContext != null)
            {
                try { await _hubContext.Clients.Group(order.CustomerId.ToString()).SendAsync("OrderRejected", new { OrderId = order.Id }); } catch { }
            }

            _context.AuditLogs.Add(new AuditLog
            {
                EntityName = "Order",
                EntityId = order.Id,
                Action = "Reject",
                PerformedBy = "Admin",
                PerformedAt = DateTime.UtcNow,
                Details = $"Order {order.OrderNumber} rejected, stock restored"
            });

            await _context.SaveChangesAsync();
            return await MapToResponseDtoAsync(updated);
        }

        public async Task<OrderResponseDto> CancelOrderAsync(Guid orderId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await BeginTransactionIfSupportedAsync();
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.Id == orderId);
                    if (order == null) throw new KeyNotFoundException("Order not found");

                    if (order.Status == "Cancelled") return await MapToResponseDtoAsync(order);

                    foreach (var item in order.OrderItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;

                            var tx = new InventoryTransaction
                            {
                                ProductId = product.Id,
                                QuantityChange = item.Quantity,
                                Reason = "Order cancelled - stock restored",
                                CreatedBy = "system"
                            };
                            _context.InventoryTransactions.Add(tx);

                            if (_hubContext != null)
                            {
                                try { await _hubContext.Clients.All.SendAsync("InventoryUpdated", new { ProductId = product.Id, NewQuantity = product.StockQuantity }); } catch { }
                            }
                        }
                    }

                    order.Status = "Cancelled";
                    await _orderRepository.UpdateAsync(order, saveChanges: false);
                    await _context.SaveChangesAsync();
                    if (transaction != null) await transaction.CommitAsync();

                    if (_hubContext != null)
                    {
                        try { await _hubContext.Clients.Group(order.CustomerId.ToString()).SendAsync("OrderCancelled", new { OrderId = order.Id }); } catch { }
                    }

                    return await MapToResponseDtoAsync(order);
                }
                catch
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrdersWithDetailsAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .ToListAsync();

            var dtos = new List<OrderResponseDto>();
            foreach (var order in orders)
            {
                dtos.Add(await MapToResponseDtoAsync(order));
            }
            return dtos;
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) throw new KeyNotFoundException("Order not found");
            return await MapToResponseDtoAsync(order);
        }

        public async Task DispatchViaCourierAsync(Guid orderId, string courierType)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            order.Status = "Dispatched";
            order.CourierRef = $"{courierType}-{orderId.ToString().Substring(0, 8)}";
            await _orderRepository.UpdateAsync(order);
        }

        public async Task<OrderResponseDto> ReceiveOrderAsync(Guid orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) throw new KeyNotFoundException("Order not found");

            order.Status = "Received";
            var updated = await _orderRepository.UpdateAsync(order);
            return await MapToResponseDtoAsync(updated);
        }

        private async Task<OrderResponseDto> MapToResponseDtoAsync(Order order)
        {
            await _context.Entry(order).Reference(o => o.Customer).LoadAsync();
            if (order.EmployeeId.HasValue)
                await _context.Entry(order).Reference(o => o.Employee).LoadAsync();
            await _context.Entry(order).Collection(o => o.OrderItems).LoadAsync();

            foreach (var item in order.OrderItems)
            {
                await _context.Entry(item).Reference(oi => oi.Product).LoadAsync();
            }

            var employeeName = order.Employee?.FullName ?? string.Empty;

            return new OrderResponseDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName ?? string.Empty,
                EmployeeId = order.EmployeeId,
                EmployeeName = employeeName,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                TaxAmount = order.TaxAmount,
                DiscountAmount = order.DiscountAmount,
                DeliveryCity = order.DeliveryCity ?? string.Empty,
                DeliveryAddress = order.DeliveryAddress ?? string.Empty,
                DeliveryPeriod = order.DeliveryPeriod ?? string.Empty,
                PaymentMethod = order.PaymentMethod ?? string.Empty,
                CourierRef = order.CourierRef ?? string.Empty,
                Items = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product == null ? string.Empty : oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 3).ToUpper()}";
        }

        private async Task CheckAndCreateAlertsAsync()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= p.ReorderLevel && !p.IsDeleted)
                .ToListAsync();

            foreach (var product in lowStockProducts)
            {
                var existingAlert = await _context.StockAlerts
                    .FirstOrDefaultAsync(sa => sa.ProductId == product.Id && !sa.IsResolved);

                if (existingAlert == null)
                {
                    var alert = new StockAlert
                    {
                        ProductId = product.Id,
                        CurrentStock = product.StockQuantity,
                        ReorderLevel = product.ReorderLevel
                    };
                    _context.StockAlerts.Add(alert);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<IDbContextTransaction?> BeginTransactionIfSupportedAsync()
        {
            if (_context.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true)
            {
                return null;
            }

            return await _context.Database.BeginTransactionAsync();
        }
    }
}