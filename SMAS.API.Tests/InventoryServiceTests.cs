using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SMAS.API.Data;
using SMAS.API.DTOs;
using SMAS.API.Hubs;
using SMAS.API.Models;
using SMAS.API.Repositories;
using SMAS.API.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.SignalR;

namespace SMAS.API.Tests
{
    // Minimal fake hub context that no-ops SendAsync calls
    class FakeClientProxy : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[]? args, System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    class FakeHubClients : IHubClients
    {
        public IClientProxy All => new FakeClientProxy();

        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();

        public IClientProxy Client(string connectionId) => new FakeClientProxy();

        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new FakeClientProxy();

        public IClientProxy Group(string groupName) => new FakeClientProxy();

        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();

        public IClientProxy Groups(IReadOnlyList<string> groupNames) => new FakeClientProxy();

        public IClientProxy User(string userId) => new FakeClientProxy();

        public IClientProxy Users(IReadOnlyList<string> userIds) => new FakeClientProxy();
    }

    class FakeHubContext : IHubContext<NotificationHub>
    {
        public IHubClients Clients { get; } = new FakeHubClients();

        public IGroupManager Groups { get; } = null!; // not used in tests
    }

    public class InventoryServiceTests
    {
        private SmasDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SmasDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new SmasDbContext(options);
        }

        [Fact]
        public async Task CreateProduct_Should_Persist_And_Prevent_Duplicate_SKU()
        {
            using var context = CreateContext("CreateProductDb");

            // seed a category
            var category = new Category { Name = "Phones" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var cache = new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()));
            var svc = new InventoryService(repo, context, new FakeHubContext(), cache);

            var dto = new ProductCreateDto
            {
                Name = "Test Phone",
                SKU = "SKU-001",
                CategoryId = category.Id,
                UnitPrice = 100,
                PurchasePrice = 80,
                StockQuantity = 10,
                ReorderLevel = 5,
                TaxPercentage = 0
            };

            var created = await svc.CreateProductAsync(dto);
            Assert.Equal("SKU-001", created.SKU);

            // Attempt to create another with same SKU should throw
            var dto2 = new ProductCreateDto
            {
                Name = "Another",
                SKU = "SKU-001",
                CategoryId = category.Id,
                UnitPrice = 50,
                PurchasePrice = 40,
                StockQuantity = 5,
                ReorderLevel = 2,
                TaxPercentage = 0
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateProductAsync(dto2));
        }

        [Fact]
        public async Task UpdateProduct_Should_Prevent_Duplicate_SKU()
        {
            using var context = CreateContext("UpdateProductDb");
            var category = new Category { Name = "Phones" };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var p1 = new Product { Name = "P1", SKU = "S1", CategoryId = category.Id, UnitPrice = 10, PurchasePrice = 8, StockQuantity = 1, ReorderLevel = 0 };
            var p2 = new Product { Name = "P2", SKU = "S2", CategoryId = category.Id, UnitPrice = 20, PurchasePrice = 15, StockQuantity = 2, ReorderLevel = 0 };
            context.Products.AddRange(p1, p2);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var cache = new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()));
            var svc = new InventoryService(repo, context, new FakeHubContext(), cache);

            var updateDto = new ProductUpdateDto
            {
                Name = "P2 updated",
                SKU = "S1", // collide with p1
                CategoryId = category.Id,
                UnitPrice = 25,
                PurchasePrice = 20,
                StockQuantity = 3,
                ReorderLevel = 0,
                TaxPercentage = 0
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.UpdateProductAsync(p2.Id, updateDto));
        }

        [Fact]
        public async Task Create_And_Update_Should_Require_Category()
        {
            using var context = CreateContext("RequireCategoryDb");
            var repo = new ProductRepository(context);
            var cache = new MemoryCacheService(new MemoryCache(new MemoryCacheOptions()));
            var svc = new InventoryService(repo, context, new FakeHubContext(), cache);

            var dto = new ProductCreateDto
            {
                Name = "NoCat",
                SKU = "NC-1",
                CategoryId = Guid.Empty,
                UnitPrice = 1,
                PurchasePrice = 1,
                StockQuantity = 1,
                ReorderLevel = 0,
                TaxPercentage = 0
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateProductAsync(dto));

            // For update, seed a product first
            var cat = new Category { Name = "C" }; context.Categories.Add(cat); await context.SaveChangesAsync();
            var p = new Product { Name = "X", SKU = "X1", CategoryId = cat.Id, UnitPrice = 1, PurchasePrice = 1, StockQuantity = 1, ReorderLevel = 0 };
            context.Products.Add(p); await context.SaveChangesAsync();

            var updateDto = new ProductUpdateDto
            {
                Name = "X",
                SKU = "X1",
                CategoryId = Guid.Empty,
                UnitPrice = 1,
                PurchasePrice = 1,
                StockQuantity = 1,
                ReorderLevel = 0,
                TaxPercentage = 0
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.UpdateProductAsync(p.Id, updateDto));
        }
    }
}
