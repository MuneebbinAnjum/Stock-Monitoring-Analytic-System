using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace SMAS.API.Services
{
    public class AlertService : IAlertService
    {
        private readonly SmasDbContext _context;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<SMAS.API.Hubs.NotificationHub> _hubContext;

        public AlertService(SmasDbContext context, Microsoft.AspNetCore.SignalR.IHubContext<SMAS.API.Hubs.NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<StockAlertResponseDto>> GetUnresolvedAlertsAsync()
        {
            var alerts = await _context.StockAlerts
                .Where(sa => !sa.IsResolved)
                .Include(sa => sa.Product)
                .ToListAsync();

            return alerts.Select(sa => new StockAlertResponseDto
            {
                Id = sa.Id,
                ProductId = sa.ProductId,
                ProductName = sa.Product?.Name ?? string.Empty,
                TriggeredAt = sa.TriggeredAt,
                CurrentStock = sa.CurrentStock,
                ReorderLevel = sa.ReorderLevel,
                IsResolved = sa.IsResolved
            });
        }

        public async Task<IEnumerable<StockAlertResponseDto>> GetAllAlertsAsync()
        {
            var alerts = await _context.StockAlerts
                .Include(sa => sa.Product)
                .ToListAsync();

            return alerts.Select(sa => new StockAlertResponseDto
            {
                Id = sa.Id,
                ProductId = sa.ProductId,
                ProductName = sa.Product?.Name ?? string.Empty,
                TriggeredAt = sa.TriggeredAt,
                CurrentStock = sa.CurrentStock,
                ReorderLevel = sa.ReorderLevel,
                IsResolved = sa.IsResolved
            });
        }

        public async Task ResolveAlertAsync(Guid alertId)
        {
            var alert = await _context.StockAlerts.FindAsync(alertId);
            if (alert == null) throw new KeyNotFoundException("Alert not found");

            alert.IsResolved = true;
            alert.UpdatedAt = DateTime.UtcNow;
            
            // Explicitly mark entity as modified to ensure EF Core tracks changes
            _context.Update(alert);
            await _context.SaveChangesAsync();
            if (_hubContext != null)
            {
                try { await _hubContext.Clients.All.SendAsync("StockAlertResolved", new { AlertId = alertId, ProductId = alert.ProductId }); } catch { }
            }
        }

        public async Task TriggerAlertsCheckAsync()
        {
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= p.ReorderLevel)
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
                    if (_hubContext != null)
                    {
                        try { await _hubContext.Clients.All.SendAsync("StockAlertCreated", new { ProductId = product.Id, CurrentStock = product.StockQuantity }); } catch { }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}