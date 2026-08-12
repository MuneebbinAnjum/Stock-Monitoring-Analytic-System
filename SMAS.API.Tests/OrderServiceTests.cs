using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SMAS.API.Data;
using SMAS.API.Models;
using SMAS.API.Repositories;
using SMAS.API.Services;
using SMAS.API.DTOs;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace SMAS.API.Tests
{
    public class OrderServiceTests
    {
        [Fact]
        public async Task CreateOrder_ShouldReduceStock()
        {
            var options = new DbContextOptionsBuilder<SmasDbContext>()
                .UseInMemoryDatabase(databaseName: "OrderTestDb")
                .Options;

            using (var context = new SmasDbContext(options))
            {
                var product = new Product { Name = "Test Product", SKU = "TP01", UnitPrice = 10m, StockQuantity = 10, ReorderLevel = 2 };
                context.Products.Add(product);
                var customer = new Customer { FullName = "Cust", Email = "c@example.com" };
                context.Customers.Add(customer);
                await context.SaveChangesAsync();

                var orderRepo = new OrderRepository(context);
                var orderService = new OrderService(orderRepo, context, null);

                var items = new List<OrderItemDto> { new OrderItemDto { ProductId = product.Id, Quantity = 3 } };
                var dto = new OrderCreateDto { CustomerId = customer.Id, Items = items };
                var result = await orderService.CreateOrderAsync(dto);

                var updated = await context.Products.FindAsync(product.Id);
                Assert.Equal(7, updated.StockQuantity);
            }
        }
    }
}
