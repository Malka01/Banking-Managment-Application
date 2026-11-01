IF DB_ID(N'BankCoreDb') IS NULL
    CREATE DATABASE BankCoreDb;
GO
USE BankCoreDb;
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers (
        Id INT IDENTITY PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Nic NVARCHAR(20) NOT NULL UNIQUE,
        Email NVARCHAR(254) NOT NULL,
        PhoneNumber NVARCHAR(30) NOT NULL,
        Address NVARCHAR(300) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        IsActive BIT NOT NULL DEFAULT 1
    );
    CREATE TABLE dbo.Accounts (
        Id INT IDENTITY PRIMARY KEY,
        AccountNumber NVARCHAR(20) NOT NULL UNIQUE,
        CustomerId INT NOT NULL REFERENCES dbo.Customers(Id),
        AccountType NVARCHAR(30) NOT NULL,
        Balance DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Balance >= 0),
        Currency CHAR(3) NOT NULL DEFAULT 'LKR',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    CREATE TABLE dbo.Transactions (
        Id BIGINT IDENTITY PRIMARY KEY,
        ReferenceNumber NVARCHAR(40) NOT NULL,
        AccountId INT NOT NULL REFERENCES dbo.Accounts(Id),
        Type NVARCHAR(20) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL CHECK (Amount > 0),
        PreviousBalance DECIMAL(18,2) NOT NULL,
        NewBalance DECIMAL(18,2) NOT NULL CHECK (NewBalance >= 0),
        Description NVARCHAR(250) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
    CREATE TABLE dbo.AuditLogs (
        Id BIGINT IDENTITY PRIMARY KEY,
        Action NVARCHAR(100) NOT NULL,
        EntityName NVARCHAR(100) NOT NULL,
        EntityId INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO
