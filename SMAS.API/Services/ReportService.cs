using Microsoft.EntityFrameworkCore;
using SMAS.API.Data;
using SMAS.API.DTOs;

namespace SMAS.API.Services
{
    public class ReportService : IReportService
    {
        private readonly SmasDbContext _context;

        public ReportService(SmasDbContext context)
        {
            _context = context;
        }

        public async Task<SalesSummaryDto> GenerateSalesSummaryAsync(DateRangeDto dateRange)
        {
            var sales = await _context.SaleRecords
                .Where(sr => sr.SaleDate >= dateRange.StartDate && sr.SaleDate <= dateRange.EndDate)
                .ToListAsync();

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= dateRange.StartDate && o.OrderDate <= dateRange.EndDate)
                .ToListAsync();

            var totalRevenue = sales.Sum(sr => sr.Revenue);
            var totalUnits = sales.Sum(sr => sr.QuantitySold);

            var dailyRevenue = sales
                .GroupBy(sr => sr.SaleDate.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.Sum(sr => sr.Revenue));

            var weeklyRevenue = sales
                .GroupBy(sr => $"{sr.SaleDate.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(sr.SaleDate)}")
                .ToDictionary(g => g.Key, g => g.Sum(sr => sr.Revenue));

            var monthlyRevenue = sales
                .GroupBy(sr => sr.SaleDate.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Sum(sr => sr.Revenue));

            return new SalesSummaryDto
            {
                TotalOrders = orders.Count,
                TotalRevenue = totalRevenue,
                TotalUnitsSold = totalUnits,
                DailyRevenue = dailyRevenue,
                WeeklyRevenue = weeklyRevenue,
                MonthlyRevenue = monthlyRevenue
            };
        }

        public async Task<InventoryTurnoverDto> GenerateInventoryTurnoverReportAsync()
        {
            var products = await _context.Products.Include(p => p.SaleRecords).ToListAsync();

            var productTurnovers = new List<ProductTurnoverDto>();
            decimal totalTurnover = 0;

            foreach (var product in products)
            {
                var unitsSold = product.SaleRecords?.Sum(sr => sr.QuantitySold) ?? 0;
                var averageInventory = (product.StockQuantity + (product.SaleRecords?.Count > 0 ? (decimal)product.SaleRecords.Average(sr => sr.QuantitySold) : 0m)) / 2m;
                var turnoverRatio = averageInventory > 0 ? unitsSold / averageInventory : 0;

                productTurnovers.Add(new ProductTurnoverDto
                {
                    ProductName = product.Name,
                    UnitsSold = unitsSold,
                    AverageInventory = averageInventory,
                    TurnoverRatio = turnoverRatio
                });

                totalTurnover += turnoverRatio;
            }

            return new InventoryTurnoverDto
            {
                TurnoverRatio = totalTurnover / products.Count,
                ProductTurnovers = productTurnovers.OrderByDescending(pt => pt.TurnoverRatio).ToList()
            };
        }

        public async Task<IEnumerable<RevenueBreakdownDto>> GenerateRevenueBreakdownAsync(string groupBy)
        {
            IQueryable<RevenueBreakdownDto> query;

            switch (groupBy.ToLower())
            {
                case "category":
                    query = from sr in _context.SaleRecords
                            join p in _context.Products on sr.ProductId equals p.Id
                            join c in _context.Categories on p.CategoryId equals c.Id
                            group sr by c.Name into g
                            select new RevenueBreakdownDto
                            {
                                GroupName = g.Key,
                                Revenue = g.Sum(sr => sr.Revenue)
                            };
                    break;
                case "supplier":
                    query = from sr in _context.SaleRecords
                            join p in _context.Products on sr.ProductId equals p.Id
                            join s in _context.Suppliers on p.SupplierId equals s.Id
                            group sr by s.CompanyName into g
                            select new RevenueBreakdownDto
                            {
                                GroupName = g.Key,
                                Revenue = g.Sum(sr => sr.Revenue)
                            };
                    break;
                case "employee":
                    query = from sr in _context.SaleRecords
                            join e in _context.Employees on sr.EmployeeId equals e.Id
                            group sr by e.FullName into g
                            select new RevenueBreakdownDto
                            {
                                GroupName = g.Key,
                                Revenue = g.Sum(sr => sr.Revenue)
                            };
                    break;
                default:
                    throw new ArgumentException("Invalid groupBy parameter");
            }

            var results = await query.ToListAsync();
            var totalRevenue = results.Sum(r => r.Revenue);

            foreach (var result in results)
            {
                result.Percentage = totalRevenue > 0 ? (result.Revenue / totalRevenue) * 100 : 0;
            }

            return results.OrderByDescending(r => r.Revenue);
        }

        public async Task<string> ExportReportAsync(string reportType, string format, DateRangeDto dateRange)
        {
            if (reportType.Equals("sales", StringComparison.OrdinalIgnoreCase))
            {
                var sales = await _context.SaleRecords
                    .Where(sr => sr.SaleDate >= dateRange.StartDate && sr.SaleDate <= dateRange.EndDate)
                    .Include(sr => sr.Product)
                    .Include(sr => sr.Employee)
                    .ToListAsync();

                if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("SaleDate,Product,Employee,Quantity,Revenue");
                    foreach (var s in sales)
                    {
                        var line = $"{s.SaleDate:O},\"{s.Product?.Name ?? string.Empty}\",\"{s.Employee?.FullName ?? string.Empty}\",{s.QuantitySold},{s.Revenue}";
                        sb.AppendLine(line);
                    }
                    return sb.ToString();
                }

                if (format.Equals("excel", StringComparison.OrdinalIgnoreCase) || format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    using var wb = new ClosedXML.Excel.XLWorkbook();
                    var ws = wb.Worksheets.Add("Sales");
                    ws.Cell(1, 1).Value = "SaleDate";
                    ws.Cell(1, 2).Value = "Product";
                    ws.Cell(1, 3).Value = "Employee";
                    ws.Cell(1, 4).Value = "Quantity";
                    ws.Cell(1, 5).Value = "Revenue";
                    var row = 2;
                    foreach (var s in sales)
                    {
                        ws.Cell(row, 1).Value = s.SaleDate;
                        ws.Cell(row, 2).Value = s.Product?.Name ?? string.Empty;
                        ws.Cell(row, 3).Value = s.Employee?.FullName ?? string.Empty;
                        ws.Cell(row, 4).Value = s.QuantitySold;
                        ws.Cell(row, 5).Value = s.Revenue;
                        row++;
                    }

                    using var ms = new System.IO.MemoryStream();
                    wb.SaveAs(ms);
                    var bytes = ms.ToArray();
                    // return base64 to the caller; controller will convert to file response
                    return Convert.ToBase64String(bytes);
                }
            }

            // Fallback: simple text
            var data = await GenerateSalesSummaryAsync(dateRange);
            return $"Report Type: {reportType}, Format: {format}, Total Revenue: {data.TotalRevenue}";
        }

        public async Task<IEnumerable<RevenueBreakdownDto>> GenerateSalesByLocationAsync()
        {
            var results = await (from o in _context.Orders
                                 where o.Status == "Received" || o.Status == "Delivered" || o.Status == "Approved" || o.Status == "Dispatched"
                                 group o by o.DeliveryCity into g
                                 select new RevenueBreakdownDto
                                 {
                                     GroupName = g.Key ?? "Unknown",
                                     Revenue = g.Sum(o => o.TotalAmount)
                                 }).ToListAsync();

            var totalRevenue = results.Sum(r => r.Revenue);
            foreach (var result in results)
            {
                result.Percentage = totalRevenue > 0 ? (result.Revenue / totalRevenue) * 100 : 0;
            }

            return results.OrderByDescending(r => r.Revenue);
        }
    }
}