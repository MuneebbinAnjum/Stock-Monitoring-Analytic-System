-- Ensure extension for uuid generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- RefreshTokens
CREATE TABLE IF NOT EXISTS "RefreshTokens" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Token" text NOT NULL UNIQUE,
    "Email" text NOT NULL,
    "Role" text NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "CreatedByIp" text,
    "RevokedAt" timestamp with time zone,
    "RevokedByIp" text,
    "ReplacedByToken" text,
    "IsDeleted" boolean DEFAULT false,
    "UpdatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS IX_RefreshTokens_Token ON "RefreshTokens" ("Token");

-- InventoryTransactions
CREATE TABLE IF NOT EXISTS "InventoryTransactions" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "ProductId" uuid NOT NULL,
    "QuantityChange" integer NOT NULL,
    "Reason" text,
    "RelatedOrderId" uuid,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" text,
    "IsDeleted" boolean DEFAULT false,
    "UpdatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_InventoryTransactions_Product FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_InventoryTransactions_ProductId ON "InventoryTransactions" ("ProductId");

-- AuditLogs
CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    "EntityName" text NOT NULL,
    "EntityId" uuid,
    "Action" text NOT NULL,
    "PerformedBy" text,
    "PerformedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    "Details" text,
    "IsDeleted" boolean DEFAULT false,
    "UpdatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS IX_AuditLogs_EntityName ON "AuditLogs" ("EntityName");
