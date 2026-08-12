using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; // Required for GetService extension method
using Microsoft.Extensions.Logging;
using SMAS.API.Models;

namespace SMAS.API.Data
{
    public static class DbSeeder
    {
        public static void Seed(SmasDbContext context)
        {
            // FIX: Using SmasDbContext as the logger category instead of the static DbSeeder class
            var logger = context.GetService<ILoggerFactory>()?.CreateLogger("DbSeeder");
            
            try
            {
                // 1. Seed Categories
                var categories = new List<Category>();
                if (!context.Categories.Any())
                {
                    logger?.LogInformation("Seeding categories...");
                    categories.AddRange(new[]
                    {
                        new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Laptops, Mobiles, and smart devices", ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?auto=format&fit=crop&w=200&q=80" },
                        new Category { Id = Guid.NewGuid(), Name = "Home Appliances", Description = "Refrigerators, ACs, Microwaves", ImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?auto=format&fit=crop&w=200&q=80" },
                        new Category { Id = Guid.NewGuid(), Name = "Fashion", Description = "Clothing, shoes, accessories", ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?auto=format&fit=crop&w=200&q=80" }
                    });
                    context.Categories.AddRange(categories);
                    context.SaveChanges();
                    logger?.LogInformation("✓ Categories seeded (3 items)");
                }
                else
                {
                    logger?.LogInformation("Categories already exist, skipping...");
                    categories = context.Categories.ToList();
                }

                // 2. Seed Suppliers
                var supplier = context.Suppliers.FirstOrDefault();
                if (supplier == null)
                {
                    logger?.LogInformation("Seeding suppliers...");
                    supplier = new Supplier
                    {
                        Id = Guid.NewGuid(),
                        CompanyName = "Global Distributors Ltd",
                        ContactName = "John Doe",
                        Phone = "+923001234567",
                        City = "Karachi",
                        Country = "Pakistan"
                    };
                    context.Suppliers.Add(supplier);
                    context.SaveChanges();
                    logger?.LogInformation("✓ Supplier seeded");
                }

                // 3. Seed Products
                if (!context.Products.Any() && categories.Count > 0 && supplier != null)
                {
                    logger?.LogInformation("Seeding products...");
                    var products = new[]
                    {
                        new Product
                        {
                            Id = Guid.NewGuid(),
                            Name = "Sony Bravia 55\" 4K Ultra HD Smart TV",
                            SKU = "SONY-55-4K",
                            CategoryId = categories.FirstOrDefault(c => c.Name == "Electronics")?.Id ?? Guid.Empty,
                            SupplierId = supplier.Id,
                            UnitPrice = 125000, PurchasePrice = 95000, DiscountPrice = 115000,
                            TaxPercentage = 17,
                            StockQuantity = 12, ReorderLevel = 3,
                            Description = "Experience stunning 4K Ultra HD visual clarity with high dynamic range (HDR) and smart assistant integration.",
                            BrandName = "Sony", CompanyName = "Sony Electronics Inc.", Model = "KD-55X75K",
                            DeliveryPeriod = "2-4 Business Days",
                            WarrantyInfo = "2 Years Manufacturer Warranty",
                            Weight = "15.5 kg", Dimensions = "123.2 x 71.3 x 7.1 cm",
                            Tags = "tv,smart-tv,4k,sony,electronics",
                            IsFeatured = true, Status = "Active",
                            ProductImages = new List<ProductImage>
                            {
                                new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1593784991095-a205069470b6?auto=format&fit=crop&w=600&q=80", DisplayOrder = 0, AltText = "Sony Bravia 55 4K Ultra HD Smart TV" }
                            }
                        },
                        new Product
                        {
                            Id = Guid.NewGuid(),
                            Name = "ErgoFit Premium Mesh Office Chair",
                            SKU = "ERGO-CH-01",
                            CategoryId = categories.FirstOrDefault(c => c.Name == "Home Appliances")?.Id ?? Guid.Empty,
                            SupplierId = supplier.Id,
                            UnitPrice = 18500, PurchasePrice = 12000, DiscountPrice = null,
                            TaxPercentage = 17,
                            StockQuantity = 4, ReorderLevel = 5,
                            Description = "Ergonomically designed office chair with lumbar support, 3D armrests, and high-density mesh back.",
                            BrandName = "ErgoFit", CompanyName = "Comfort Seating Corp.", Model = "EF-200-Mesh",
                            DeliveryPeriod = "3-5 Business Days",
                            WarrantyInfo = "1 Year Warranty",
                            Weight = "12 kg", Dimensions = "68 x 65 x 120 cm",
                            Tags = "chair,office,ergonomic,mesh",
                            IsFeatured = false, Status = "Active",
                            ProductImages = new List<ProductImage>
                            {
                                new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1505797149-43b0069ec26b?auto=format&fit=crop&w=600&q=80", DisplayOrder = 0, AltText = "ErgoFit Premium Mesh Office Chair" }
                            }
                        },
                        new Product
                        {
                            Id = Guid.NewGuid(),
                            Name = "Urban Leather Bomber Jacket",
                            SKU = "BOMBER-JKT-L",
                            CategoryId = categories.FirstOrDefault(c => c.Name == "Fashion")?.Id ?? Guid.Empty,
                            SupplierId = supplier.Id,
                            UnitPrice = 8500, PurchasePrice = 4500, DiscountPrice = 6999,
                            TaxPercentage = 5,
                            StockQuantity = 25, ReorderLevel = 8,
                            Description = "Stylish wind-resistant genuine black leather jacket with soft lining and premium zippers.",
                            BrandName = "UrbanStyle", CompanyName = "Urban Outfitters", Model = "UB-Bomb-09",
                            DeliveryPeriod = "1-3 Business Days",
                            WarrantyInfo = "6 Months",
                            Weight = "1.2 kg", Dimensions = "N/A",
                            Tags = "jacket,leather,fashion,bomber,men",
                            IsFeatured = true, Status = "Active",
                            ProductImages = new List<ProductImage>
                            {
                                new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1551028719-00167b16eac5?auto=format&fit=crop&w=600&q=80", DisplayOrder = 0, AltText = "Urban Leather Bomber Jacket" }
                            }
                        }
                    };
                    context.Products.AddRange(products);
                    context.SaveChanges();
                    logger?.LogInformation("✓ Products seeded (3 items)");
                }
                else if (context.Products.Any())
                {
                    logger?.LogInformation("Products already exist, skipping...");
                }

                // 4. Seed Employees (Admin & Salesman)
                logger?.LogInformation("Seeding employees...");
                var admin = context.Employees.FirstOrDefault(e => e.Role == "Admin");
                if (admin == null)
                {
                    context.Employees.Add(new Employee
                    {
                        Id = Guid.NewGuid(), FullName = "System Admin", Email = "admin@smas.com",
                        Role = "Admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                        ApprovalStatus = "Approved", HireDate = DateTime.UtcNow, MonthlySalesTarget = 500000
                    });
                    logger?.LogInformation("  Added Admin user: admin@smas.com (password: Admin123)");
                }
                else
                {
                    logger?.LogInformation("  Admin user already exists: {Email}, skipping password update", admin.Email);
                }

                var salesman = context.Employees.FirstOrDefault(e => e.Role == "Salesman");
                if (salesman == null)
                {
                    context.Employees.Add(new Employee
                    {
                        Id = Guid.NewGuid(), FullName = "Demo Salesman", Email = "salesman@smas.com",
                        Role = "Salesman", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                        ApprovalStatus = "Approved", HireDate = DateTime.UtcNow, MonthlySalesTarget = 200000
                    });
                    logger?.LogInformation("  Added Salesman user: salesman@smas.com (password: Admin123)");
                }
                else
                {
                    logger?.LogInformation("  Salesman user already exists: {Email}, skipping password update", salesman.Email);
                }
                context.SaveChanges();
                logger?.LogInformation("✓ Employees seeded");

                // 5. Seed Customers (Buyer & Walk-in Customer)
                if (!context.Customers.Any())
                {
                    logger?.LogInformation("Seeding customers...");
                    context.Customers.AddRange(
                        new Customer
                        {
                            Id = Guid.NewGuid(), FullName = "Demo Buyer", Email = "buyer@smas.com",
                            Phone = "03331112223", City = "Karachi", Province = "Sindh",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"), IsActive = true
                        },
                        new Customer
                        {
                            Id = Guid.NewGuid(), FullName = "Walk-in Customer", Email = "walkin@smas.com",
                            Phone = "00000000000", City = "Local Store", Province = "Local Store",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"), IsActive = true
                        }
                    );
                    context.SaveChanges();
                    logger?.LogInformation("✓ Customers seeded (2 items)");
                    logger?.LogInformation("  - buyer@smas.com (password: Admin123) - Role: Buyer");
                    logger?.LogInformation("  - walkin@smas.com (password: Admin123) - Role: Buyer");
                }
                else
                {
                    logger?.LogInformation("Customers already exist, skipping...");
                }

                // Additional mock data: ensure at least 5 buyers and 20 products, create orders and complaints
                // Add more customers (buyers)
                var currentCustomers = context.Customers.ToList();
                var buyerCount = currentCustomers.Count;
                var buyersToAdd = 5 - buyerCount;
                if (buyersToAdd > 0)
                {
                    logger?.LogInformation("Adding {Count} additional demo buyers...", buyersToAdd);
                    for (int i = 1; i <= buyersToAdd; i++)
                    {
                        var c = new Customer
                        {
                            Id = Guid.NewGuid(),
                            FullName = $"Demo Buyer {i}",
                            Email = $"buyer{i}@smas.com",
                            Phone = $"0300{100000 + i}",
                            City = "Karachi",
                            Province = "Sindh",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                            IsActive = true
                        };
                        context.Customers.Add(c);
                    }
                    context.SaveChanges();
                    logger?.LogInformation("✓ Added {Count} buyers", buyersToAdd);
                }

                // Ensure we have at least 20 products
                var allProducts = context.Products.ToList();
                var needProducts = 20 - allProducts.Count;
                if (needProducts > 0 && supplier != null && categories.Any())
                {
                    logger?.LogInformation("Adding {Count} demo products...", needProducts);
                    var rnd = new Random();
                    for (int i = 1; i <= needProducts; i++)
                    {
                        var cat = categories[rnd.Next(categories.Count)];
                        var p = new Product
                        {
                            Id = Guid.NewGuid(),
                            Name = $"Demo Product {allProducts.Count + i}",
                            SKU = $"DEMO-P-{allProducts.Count + i}",
                            CategoryId = cat.Id,
                            SupplierId = supplier.Id,
                            UnitPrice = 1000 + rnd.Next(100, 50000),
                            PurchasePrice = 800 + rnd.Next(50, 40000),
                            DiscountPrice = null,
                            TaxPercentage = 17,
                            StockQuantity = rnd.Next(5, 100),
                            ReorderLevel = 5,
                            Description = "Demo product for testing and seeding purposes.",
                            BrandName = "DemoBrand",
                            CompanyName = "DemoCompany",
                            Model = $"DM-{rnd.Next(1000,9999)}",
                            DeliveryPeriod = "2-5 Business Days",
                            WarrantyInfo = "6 Months",
                            Weight = "0.5 kg",
                            Dimensions = "10 x 5 x 2 cm",
                            Tags = "demo,test,seed",
                            IsFeatured = false,
                            Status = "Active",
                            ProductImages = new List<ProductImage>
                            {
                                new ProductImage { Id = Guid.NewGuid(), ImageUrl = "https://images.unsplash.com/photo-1581291518835-1f5f9d8d0d3f?auto=format&fit=crop&w=600&q=80", DisplayOrder = 0, AltText = "Demo product image" }
                            }
                        };
                        context.Products.Add(p);
                    }
                    context.SaveChanges();
                    logger?.LogInformation("✓ Added {Count} demo products", needProducts);
                }

                // Create sample orders for buyers
                var customersForOrders = context.Customers.Take(5).ToList();
                var productsForOrders = context.Products.Take(15).ToList();
                if (customersForOrders.Any() && productsForOrders.Any())
                {
                    logger?.LogInformation("Creating demo orders for buyers...");
                    var rnd = new Random();
                    foreach (var cust in customersForOrders)
                    {
                        // create 2 orders per customer
                        for (int o = 0; o < 2; o++)
                        {
                            var baseOrder = $"D-{DateTime.UtcNow:yyMMddHHmmss}-{cust.Email.Split('@')[0]}-{o}";
                            // Append a short unique suffix to avoid duplicates, while keeping total length <= 30
                            var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
                            var maxBaseLen = 30 - 1 - suffix.Length; // reserve 1 for dash
                            var trimmedBase = baseOrder.Length > maxBaseLen ? baseOrder.Substring(0, maxBaseLen) : baseOrder;
                            var generatedOrderNumber = $"{trimmedBase}-{suffix}";
                            var order = new Order
                            {
                                Id = Guid.NewGuid(),
                                OrderNumber = generatedOrderNumber,
                                CustomerId = cust.Id,
                                OrderType = "Online",
                                OrderDate = DateTime.UtcNow,
                                Status = "Processing",
                                DeliveryCharges = 250,
                                PaymentMethod = "Cash on Delivery",
                                DeliveryCity = cust.City,
                                DeliveryAddress = "Demo Address",
                                Notes = "Demo seeded order",
                                TotalAmount = 0 // will compute
                            };
                            context.Orders.Add(order);
                            context.SaveChanges();

                            // Add 1-3 items
                            var itemCount = rnd.Next(1, 4);
                            decimal total = 0;
                            for (int it = 0; it < itemCount; it++)
                            {
                                var prod = productsForOrders[rnd.Next(productsForOrders.Count)];
                                var qty = rnd.Next(1, 5);
                                var unit = prod.UnitPrice;
                                var oi = new OrderItem
                                {
                                    Id = Guid.NewGuid(),
                                    OrderId = order.Id,
                                    ProductId = prod.Id,
                                    Quantity = qty,
                                    UnitPrice = unit
                                };
                                context.OrderItems.Add(oi);
                                total += unit * qty;
                                // reduce stock quantity
                                prod.StockQuantity = Math.Max(0, prod.StockQuantity - qty);
                            }
                            // compute tax (default 17%) and totals
                            order.TotalAmount = total + order.DeliveryCharges;
                            order.TaxAmount = Math.Round(total * 0.17m, 2);
                            context.SaveChanges();
                        }
                    }
                    logger?.LogInformation("✓ Demo orders created for buyers");
                }

                // Create complaints for some orders
                var someOrders = context.Orders.Take(5).ToList();
                if (someOrders.Any())
                {
                    logger?.LogInformation("Seeding demo complaints...");
                    var count = 0;
                    foreach (var ord in someOrders)
                    {
                        var comp = new Complaint
                        {
                            Id = Guid.NewGuid(),
                            OrderId = ord.Id,
                            CustomerId = ord.CustomerId,
                            ComplaintType = "Delivery",
                            Title = "Damaged item on arrival",
                            Description = "Item arrived damaged; packaging torn. Requesting replacement or refund.",
                            Status = "Open",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        context.Complaints.Add(comp);
                        count++;
                        if (count >= 5) break;
                    }
                    context.SaveChanges();
                    logger?.LogInformation("✓ Demo complaints created ({Count})", Math.Min(5, someOrders.Count));
                }

                // 6. Seed System Settings
                if (!context.SystemSettings.Any())
                {
                    logger?.LogInformation("Seeding system settings...");
                    context.SystemSettings.AddRange(
                        new SystemSetting { Key = "tax_percentage", Value = "17", Description = "Default tax/VAT percentage applied to orders", Category = "Tax" },
                        new SystemSetting { Key = "delivery_charges", Value = "250", Description = "Default delivery charges in PKR", Category = "Delivery" },
                        new SystemSetting { Key = "free_delivery_threshold", Value = "5000", Description = "Order amount above which delivery is free", Category = "Delivery" },
                        new SystemSetting { Key = "low_stock_threshold", Value = "5", Description = "Default low stock alert threshold", Category = "Inventory" },
                        new SystemSetting { Key = "company_name", Value = "SMAS Store", Description = "Company name shown on invoices", Category = "General" },
                        new SystemSetting { Key = "company_address", Value = "123 Business Avenue, Karachi, Pakistan", Description = "Company address on invoices", Category = "General" },
                        new SystemSetting { Key = "company_phone", Value = "+92-300-1234567", Description = "Company phone number", Category = "General" },
                        new SystemSetting { Key = "currency_symbol", Value = "Rs.", Description = "Currency symbol used in the system", Category = "General" }
                    );
                    context.SaveChanges();
                    logger?.LogInformation("✓ System settings seeded (8 items)");
                }
                else
                {
                    logger?.LogInformation("System settings already exist, skipping...");
                }

                logger?.LogInformation("═══════════════════════════════════════════════════════════════");
                logger?.LogInformation("Database seeding completed successfully!");
                logger?.LogInformation("Default credentials:");
                logger?.LogInformation("  Admin:    admin@smas.com     / Admin123");
                logger?.LogInformation("  Salesman: salesman@smas.com  / Admin123");
                logger?.LogInformation("  Buyer:    buyer@smas.com     / Admin123");
                logger?.LogInformation("═══════════════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during database seeding");
                throw;
            }
        }
    }
}