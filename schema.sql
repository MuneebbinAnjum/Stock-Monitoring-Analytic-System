CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- PHASE 2B — COMPLETE DATABASE SCHEMA

-- Categories Table
CREATE TABLE IF NOT EXISTS Categories (
    CategoryId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) NOT NULL UNIQUE,
    Description TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- Suppliers Table
CREATE TABLE IF NOT EXISTS Suppliers (
    SupplierId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CompanyName VARCHAR(100) NOT NULL,
    ContactName VARCHAR(100),
    Phone VARCHAR(20),
    City VARCHAR(50),
    Country VARCHAR(50) DEFAULT 'Pakistan',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- Products Table
CREATE TABLE IF NOT EXISTS Products (
    ProductId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(200) NOT NULL,
    SKU VARCHAR(50) UNIQUE NOT NULL,
    CategoryId UUID NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL CHECK (UnitPrice > 0),
    StockQuantity INTEGER NOT NULL DEFAULT 0 CHECK (StockQuantity >= 0),
    ReorderLevel INTEGER NOT NULL DEFAULT 10 CHECK (ReorderLevel >= 0),
    SupplierId UUID NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- Employees Table
CREATE TABLE IF NOT EXISTS Employees (
    EmployeeId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) NOT NULL,
    Role VARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Manager', 'Salesman')),
    Email VARCHAR(100) UNIQUE NOT NULL,
    HireDate DATE NOT NULL,
    MonthlySalesTarget DECIMAL(12,2) DEFAULT 0,
    PasswordHash VARCHAR(255) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- Customers Table
CREATE TABLE IF NOT EXISTS Customers (
    CustomerId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    Phone VARCHAR(20),
    City VARCHAR(50),
    Province VARCHAR(50),
    PasswordHash VARCHAR(255) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- Orders Table
CREATE TABLE IF NOT EXISTS Orders (
    OrderId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CustomerId UUID NOT NULL,
    EmployeeId UUID,
    OrderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Processing', 'Dispatched', 'Delivered', 'Cancelled')),
    TotalAmount DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (TotalAmount >= 0),
    DeliveryCity VARCHAR(50),
    CourierRef VARCHAR(100),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- OrderItems Table
CREATE TABLE IF NOT EXISTS OrderItems (
    OrderItemId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    OrderId UUID NOT NULL,
    ProductId UUID NOT NULL,
    Quantity INTEGER NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(10,2) NOT NULL CHECK (UnitPrice > 0),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- SaleRecords Table
CREATE TABLE IF NOT EXISTS SaleRecords (
    SaleId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProductId UUID NOT NULL,
    EmployeeId UUID NOT NULL,
    SaleDate DATE NOT NULL,
    QuantitySold INTEGER NOT NULL CHECK (QuantitySold > 0),
    Revenue DECIMAL(12,2) NOT NULL CHECK (Revenue > 0),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- StockAlerts Table
CREATE TABLE IF NOT EXISTS StockAlerts (
    AlertId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProductId UUID NOT NULL,
    TriggeredAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CurrentStock INTEGER NOT NULL,
    ReorderLevel INTEGER NOT NULL,
    IsResolved BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- ForecastRecords Table
CREATE TABLE IF NOT EXISTS ForecastRecords (
    ForecastId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ProductId UUID NOT NULL,
    ForecastDate DATE NOT NULL,
    PredictedDemand INTEGER NOT NULL CHECK (PredictedDemand >= 0),
    TrendScore DECIMAL(5,2) DEFAULT 0,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);

-- Foreign Key Constraints
ALTER TABLE Products ADD CONSTRAINT FK_Products_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId) ON DELETE CASCADE;
ALTER TABLE Products ADD CONSTRAINT FK_Products_SupplierId FOREIGN KEY (SupplierId) REFERENCES Suppliers(SupplierId) ON DELETE CASCADE;
ALTER TABLE Orders ADD CONSTRAINT FK_Orders_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId) ON DELETE CASCADE;
ALTER TABLE Orders ADD CONSTRAINT FK_Orders_EmployeeId FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId) ON DELETE SET NULL;
ALTER TABLE OrderItems ADD CONSTRAINT FK_OrderItems_OrderId FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE;
ALTER TABLE OrderItems ADD CONSTRAINT FK_OrderItems_ProductId FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE;
ALTER TABLE SaleRecords ADD CONSTRAINT FK_SaleRecords_ProductId FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE;
ALTER TABLE SaleRecords ADD CONSTRAINT FK_SaleRecords_EmployeeId FOREIGN KEY (EmployeeId) REFERENCES Employees(EmployeeId) ON DELETE CASCADE;
ALTER TABLE StockAlerts ADD CONSTRAINT FK_StockAlerts_ProductId FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE;
ALTER TABLE ForecastRecords ADD CONSTRAINT FK_ForecastRecords_ProductId FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE;

-- Indexes
CREATE INDEX IF NOT EXISTS IDX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IF NOT EXISTS IDX_Products_SupplierId ON Products(SupplierId);
CREATE INDEX IF NOT EXISTS IDX_Products_SKU ON Products(SKU);
CREATE INDEX IF NOT EXISTS IDX_Orders_CustomerId ON Orders(CustomerId);
CREATE INDEX IF NOT EXISTS IDX_Orders_EmployeeId ON Orders(EmployeeId);
CREATE INDEX IF NOT EXISTS IDX_Orders_Status ON Orders(Status);
CREATE INDEX IF NOT EXISTS IDX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IF NOT EXISTS IDX_OrderItems_ProductId ON OrderItems(ProductId);
CREATE INDEX IF NOT EXISTS IDX_SaleRecords_ProductId ON SaleRecords(ProductId);
CREATE INDEX IF NOT EXISTS IDX_SaleRecords_EmployeeId ON SaleRecords(EmployeeId);
CREATE INDEX IF NOT EXISTS IDX_SaleRecords_SaleDate ON SaleRecords(SaleDate);
CREATE INDEX IF NOT EXISTS IDX_StockAlerts_ProductId ON StockAlerts(ProductId);
CREATE INDEX IF NOT EXISTS IDX_StockAlerts_IsResolved ON StockAlerts(IsResolved);
CREATE INDEX IF NOT EXISTS IDX_ForecastRecords_ProductId ON ForecastRecords(ProductId);
CREATE INDEX IF NOT EXISTS IDX_ForecastRecords_ForecastDate ON ForecastRecords(ForecastDate);

-- ON DELETE Rules Explanation:
-- CASCADE: When parent is deleted, children are deleted (e.g., delete Category → delete Products)
-- SET NULL: When parent is deleted, foreign key is set to NULL (e.g., delete Employee → set Order.EmployeeId to NULL)

-- Seed Data
INSERT INTO Categories (Name, Description) VALUES
('Electronics', 'Electronic gadgets and devices'),
('Clothing', 'Apparel and fashion items'),
('Groceries', 'Food and household items')
ON CONFLICT (Name) DO NOTHING;

INSERT INTO Suppliers (CompanyName, ContactName, Phone, City, Country) VALUES
('Tech Suppliers Ltd', 'Ahmed Khan', '+92-300-1234567', 'Lahore', 'Pakistan'),
('Fashion Hub', 'Sara Ali', '+92-321-7654321', 'Karachi', 'Pakistan'),
('Grocery Mart', 'Bilal Ahmed', '+92-333-9876543', 'Islamabad', 'Pakistan'),
('Toy World', 'Fatima Noor', '+92-344-1122334', 'Faisalabad', 'Pakistan'),
('Book Depot', 'Omar Saeed', '+92-355-5566778', 'Rawalpindi', 'Pakistan')
ON CONFLICT DO NOTHING;

INSERT INTO Products (Name, SKU, CategoryId, UnitPrice, StockQuantity, ReorderLevel, SupplierId) VALUES
('Wireless Mouse', 'WM-001', 1, 1500.00, 50, 10, 1),
('LED Monitor', 'LM-002', 1, 25000.00, 20, 5, 1),
('Cotton T-Shirt', 'TS-003', 2, 800.00, 100, 20, 2),
('Jeans', 'JN-004', 2, 2500.00, 30, 8, 2),
('Rice Bag 5kg', 'RB-005', 3, 1200.00, 200, 50, 3),
('Cooking Oil 1L', 'CO-006', 3, 400.00, 150, 30, 3),
('Toy Car', 'TC-007', 4, 500.00, 80, 15, 4),
('Puzzle Game', 'PG-008', 4, 300.00, 60, 12, 4)
ON CONFLICT (SKU) DO NOTHING;

INSERT INTO Employees (Name, Role, Email, HireDate, MonthlySalesTarget, PasswordHash) VALUES
('Admin User', 'Admin', 'admin@smas.pk', '2023-01-01', 0, '$2a$11$example.hash.for.Admin@123'),
('Manager Ali', 'Manager', 'manager@smas.pk', '2023-02-01', 50000.00, '$2a$11$example.hash'),
('Salesman Ahmed', 'Salesman', 'ahmed@sales.pk', '2023-03-01', 20000.00, '$2a$11$example.hash'),
('Salesman Sara', 'Salesman', 'sara@sales.pk', '2023-04-01', 25000.00, '$2a$11$example.hash')
ON CONFLICT (Email) DO NOTHING;

INSERT INTO Customers (FullName, Email, Phone, City, Province, PasswordHash) VALUES
('John Doe', 'john@example.com', '+92-300-1111111', 'Lahore', 'Punjab', '$2a$11$customer.hash'),
('Jane Smith', 'jane@example.com', '+92-321-2222222', 'Karachi', 'Sindh', '$2a$11$customer.hash'),
('Ali Khan', 'ali@example.com', '+92-333-3333333', 'Islamabad', 'Islamabad', '$2a$11$customer.hash'),
('Sara Ahmed', 'sara@example.com', '+92-344-4444444', 'Faisalabad', 'Punjab', '$2a$11$customer.hash'),
('Bilal Noor', 'bilal@example.com', '+92-355-5555555', 'Rawalpindi', 'Punjab', '$2a$11$customer.hash'),
('Fatima Saeed', 'fatima@example.com', '+92-366-6666666', 'Multan', 'Punjab', '$2a$11$customer.hash'),
('Omar Malik', 'omar@example.com', '+92-377-7777777', 'Peshawar', 'KPK', '$2a$11$customer.hash'),
('Hina Tariq', 'hina@example.com', '+92-388-8888888', 'Quetta', 'Balochistan', '$2a$11$customer.hash'),
('Usman Iqbal', 'usman@example.com', '+92-399-9999999', 'Sialkot', 'Punjab', '$2a$11$customer.hash'),
('Ayesha Khan', 'ayesha@example.com', '+92-310-0000000', 'Gujranwala', 'Punjab', '$2a$11$customer.hash')
ON CONFLICT (Email) DO NOTHING;

INSERT INTO Orders (CustomerId, EmployeeId, OrderDate, Status, TotalAmount, DeliveryCity, CourierRef) VALUES
(1, 3, '2024-01-15', 'Delivered', 26500.00, 'Lahore', 'TCS-001'),
(2, 4, '2024-01-16', 'Processing', 3300.00, 'Karachi', NULL),
(3, 3, '2024-01-17', 'Dispatched', 1200.00, 'Islamabad', 'Leopard-002'),
(4, 4, '2024-01-18', 'Pending', 800.00, 'Faisalabad', NULL),
(5, 3, '2024-01-19', 'Delivered', 1700.00, 'Rawalpindi', 'TCS-003'),
(6, 4, '2024-01-20', 'Cancelled', 0.00, 'Multan', NULL),
(7, 3, '2024-01-21', 'Delivered', 2500.00, 'Peshawar', 'Leopard-004'),
(8, 4, '2024-01-22', 'Processing', 500.00, 'Quetta', NULL),
(9, 3, '2024-01-23', 'Dispatched', 300.00, 'Sialkot', 'TCS-005'),
(10, 4, '2024-01-24', 'Pending', 400.00, 'Gujranwala', NULL),
(1, 3, '2024-01-25', 'Delivered', 1500.00, 'Lahore', 'Leopard-006'),
(2, 4, '2024-01-26', 'Processing', 2500.00, 'Karachi', NULL),
(3, 3, '2024-01-27', 'Dispatched', 1200.00, 'Islamabad', 'TCS-007'),
(4, 4, '2024-01-28', 'Pending', 800.00, 'Faisalabad', NULL),
(5, 3, '2024-01-29', 'Delivered', 400.00, 'Rawalpindi', 'Leopard-008')
ON CONFLICT DO NOTHING;

INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES
(1, 1, 1, 1500.00), (1, 2, 1, 25000.00),
(2, 3, 2, 800.00), (2, 4, 1, 2500.00),
(3, 5, 1, 1200.00),
(4, 3, 1, 800.00),
(5, 1, 1, 1500.00), (5, 8, 1, 300.00),
(7, 4, 1, 2500.00),
(8, 7, 1, 500.00),
(9, 8, 1, 300.00),
(10, 6, 1, 400.00),
(11, 1, 1, 1500.00),
(12, 4, 1, 2500.00),
(13, 5, 1, 1200.00),
(14, 3, 1, 800.00),
(15, 6, 1, 400.00)
ON CONFLICT DO NOTHING;

INSERT INTO SaleRecords (ProductId, EmployeeId, SaleDate, QuantitySold, Revenue) VALUES
(1, 3, '2024-01-15', 5, 7500.00),
(2, 3, '2024-01-15', 2, 50000.00),
(3, 4, '2024-01-16', 10, 8000.00),
(4, 4, '2024-01-16', 3, 7500.00),
(5, 3, '2024-01-17', 8, 9600.00),
(6, 3, '2024-01-17', 12, 4800.00),
(7, 4, '2024-01-18', 15, 7500.00),
(8, 4, '2024-01-18', 20, 6000.00),
(1, 3, '2024-01-19', 7, 10500.00),
(3, 4, '2024-01-20', 5, 4000.00),
(4, 3, '2024-01-21', 4, 10000.00),
(5, 4, '2024-01-22', 6, 7200.00),
(6, 3, '2024-01-23', 9, 3600.00),
(7, 4, '2024-01-24', 11, 5500.00),
(8, 3, '2024-01-25', 8, 2400.00),
(1, 4, '2024-01-26', 6, 9000.00),
(2, 3, '2024-01-27', 1, 25000.00),
(3, 4, '2024-01-28', 4, 3200.00),
(4, 3, '2024-01-29', 2, 5000.00),
(5, 4, '2024-01-30', 3, 3600.00)
ON CONFLICT DO NOTHING;

INSERT INTO StockAlerts (ProductId, TriggeredAt, CurrentStock, ReorderLevel, IsResolved) VALUES
(1, '2024-01-10', 5, 10, FALSE),
(2, '2024-01-12', 3, 5, TRUE),
(3, '2024-01-14', 8, 20, FALSE),
(4, '2024-01-16', 6, 8, TRUE),
(5, '2024-01-18', 40, 50, FALSE)
ON CONFLICT DO NOTHING;

INSERT INTO ForecastRecords (ProductId, ForecastDate, PredictedDemand, TrendScore) VALUES
(1, '2024-02-01', 12, 1.5),
(2, '2024-02-01', 5, 0.8),
(3, '2024-02-01', 18, 2.2)
ON CONFLICT DO NOTHING;