USE BankCoreDb;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Now DATETIMEOFFSET = SYSUTCDATETIME();

INSERT INTO dbo.Customers (FullName, Nic, Email, PhoneNumber, Address, CreatedAt, IsActive)
SELECT v.FullName, v.Nic, v.Email, v.PhoneNumber, v.Address, DATEADD(DAY, v.DaysAgo, @Now), 1
FROM (VALUES
    (N'Amara Perera', N'901234567V', N'amara.perera@example.test', N'0712345678', N'Colombo', -30),
    (N'Kasun Fernando', N'921234568V', N'kasun.fernando@example.test', N'0723456789', N'Gampaha', -26),
    (N'Nadeesha Silva', N'881234569V', N'nadeesha.silva@example.test', N'0774567890', N'Kandy', -22),
    (N'Ruwan Jayasinghe', N'951234570V', N'ruwan.jayasinghe@example.test', N'0755678901', N'Galle', -18),
    (N'Dilini Wijesekara', N'891234571V', N'dilini.wijesekara@example.test', N'0766789012', N'Kurunegala', -14),
    (N'Sahan Wickramasinghe', N'971234572V', N'sahan.wickramasinghe@example.test', N'0787890123', N'Negombo', -10)
) AS v(FullName, Nic, Email, PhoneNumber, Address, DaysAgo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Customers c WHERE c.Nic = v.Nic);

INSERT INTO dbo.Accounts (AccountNumber, CustomerId, AccountType, Balance, Currency, IsActive, CreatedAt)
SELECT v.AccountNumber, c.Id, v.AccountType, v.Balance, N'LKR', 1, DATEADD(DAY, v.DaysAgo, @Now)
FROM (VALUES
    (N'1000000001', N'901234567V', N'Savings', CAST(125000.00 AS DECIMAL(18,2)), -29),
    (N'1000000002', N'921234568V', N'Current', CAST(92000.00 AS DECIMAL(18,2)), -25),
    (N'1000000003', N'881234569V', N'Savings', CAST(147500.00 AS DECIMAL(18,2)), -21),
    (N'1000000004', N'951234570V', N'Savings', CAST(63500.00 AS DECIMAL(18,2)), -17),
    (N'1000000005', N'891234571V', N'Current', CAST(91000.00 AS DECIMAL(18,2)), -13),
    (N'1000000006', N'971234572V', N'Savings', CAST(45000.00 AS DECIMAL(18,2)), -9)
) AS v(AccountNumber, Nic, AccountType, Balance, DaysAgo)
INNER JOIN dbo.Customers c ON c.Nic = v.Nic
WHERE NOT EXISTS (SELECT 1 FROM dbo.Accounts a WHERE a.AccountNumber = v.AccountNumber);

DECLARE @Transactions TABLE
(
    ReferenceNumber NVARCHAR(40) NOT NULL,
    AccountNumber NVARCHAR(20) NOT NULL,
    Type NVARCHAR(20) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    PreviousBalance DECIMAL(18,2) NOT NULL,
    NewBalance DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(250) NULL,
    DaysAgo INT NOT NULL
);

INSERT INTO @Transactions VALUES
    (N'Trans-TX-0001', N'1000000001', N'DEPOSIT',      100000.00,      0.00, 100000.00, N'Initial salary deposit', -28),
    (N'Trans-TX-0002', N'1000000001', N'DEPOSIT',       50000.00, 100000.00, 150000.00, N'Monthly savings deposit', -20),
    (N'Trans-TX-0003', N'1000000001', N'WITHDRAWAL',    25000.00, 150000.00, 125000.00, N'ATM withdrawal', -12),
    (N'Trans-TX-0004', N'1000000002', N'DEPOSIT',       75000.00,      0.00,  75000.00, N'Business deposit', -24),
    (N'Trans-TX-0005', N'1000000002', N'WITHDRAWAL',     8000.00,  75000.00,  67000.00, N'Utility payment', -16),
    (N'Trans-TX-0006', N'1000000003', N'DEPOSIT',      150000.00,      0.00, 150000.00, N'Initial deposit', -20),
    (N'Trans-TX-0007', N'1000000003', N'WITHDRAWAL',     2500.00, 150000.00, 147500.00, N'Cash withdrawal', -8),
    (N'Trans-TX-0008', N'1000000004', N'DEPOSIT',       63500.00,      0.00,  63500.00, N'Initial deposit', -16),
    (N'Trans-TX-0009', N'1000000005', N'DEPOSIT',       91000.00,      0.00,  91000.00, N'Initial deposit', -12),
    (N'Trans-TX-0010', N'1000000006', N'DEPOSIT',       45000.00,      0.00,  45000.00, N'Initial deposit', -8),
    (N'Trans-TR-0011', N'1000000001', N'TRANSFER_OUT',  25000.00, 150000.00, 125000.00, N'Trans transfer to account 1000000002', -5),
    (N'Trans-TR-0011', N'1000000002', N'TRANSFER_IN',   25000.00,  67000.00,  92000.00, N'Trans transfer from account 1000000001', -5);

INSERT INTO dbo.Transactions
    (ReferenceNumber, AccountId, Type, Amount, PreviousBalance, NewBalance, Description, CreatedAt)
SELECT t.ReferenceNumber, a.Id, t.Type, t.Amount, t.PreviousBalance, t.NewBalance, t.Description, DATEADD(DAY, t.DaysAgo, @Now)
FROM @Transactions t
INNER JOIN dbo.Accounts a ON a.AccountNumber = t.AccountNumber
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Transactions existing
    WHERE existing.ReferenceNumber = t.ReferenceNumber
      AND existing.AccountId = a.Id
);

INSERT INTO dbo.AuditLogs (Action, EntityName, EntityId, ReferenceNumber, Details, CreatedAt)
SELECT t.Type, N'Account', a.Id, t.ReferenceNumber, t.Description, DATEADD(DAY, t.DaysAgo, @Now)
FROM @Transactions t
INNER JOIN dbo.Accounts a ON a.AccountNumber = t.AccountNumber
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.AuditLogs existing
    WHERE existing.ReferenceNumber = t.ReferenceNumber
      AND existing.EntityId = a.Id
);

COMMIT TRANSACTION;
GO
