# BankCore Banking Management System

BankCore is an independently developed portfolio MVP demonstrating a React/Vite frontend and an ASP.NET Core/.NET 10 Web API backed by Microsoft SQL Server. It uses fictional data only and is **not production banking software**.

## Current MVP

- Customer registration, search/listing, and activation status
- Account opening with unique account numbers and non-negative balances
- Deposits, withdrawals, and atomic fund transfers
- Transaction history with pagination and type/account filtering
- EF Core SQL Server model with constraints and audit logs
- Responsive dashboard consuming live API responses
- Swagger/OpenAPI at `/swagger`

## Run locally

1. Install .NET SDK 10.0.403, Node.js 22, and SQL Server Developer/Express or LocalDB.
2. Set the SQL Server connection string without committing credentials:

```powershell
dotnet user-secrets --project .\backend\BankCore.Api\BankCore.Api.csproj init
dotnet user-secrets --project .\backend\BankCore.Api\BankCore.Api.csproj set "ConnectionStrings:BankCore" "Server=localhost;Database=BankCoreDb;Trusted_Connection=True;TrustServerCertificate=True"
```

3. Execute [01_CreateDatabase.sql](./database/01_CreateDatabase.sql) in SQL Server Management Studio, or let EF migrations create the schema once migrations are added.
4. Start the API:

```powershell
dotnet run --project .\backend\BankCore.Api
```

5. In another terminal, start the frontend:

```powershell
cd .\frontend
npm install
npm run dev
```

Set `VITE_API_URL` if the API uses a different HTTPS port. The default frontend URL is `https://localhost:7220/api`.

## API examples

```http
POST /api/transactions/deposit
Content-Type: application/json

{"accountNumber":"1000000001","amount":5000,"description":"Demo deposit"}
```

See Swagger for the complete request and response shapes. Financial values use `decimal(18,2)`, and transaction changes are committed in database transactions.

### Financial transaction guarantees

- Deposits, withdrawals, and transfers validate positive decimal amounts.
- Inactive accounts and insufficient withdrawals are rejected.
- Transfers debit and credit both accounts inside one SQL Server `Serializable` transaction.
- Account updates use SQL Server `rowversion` optimistic concurrency checks.
- Transfer queries lock accounts in a deterministic ID order to reduce deadlocks.
- Failed operations explicitly roll back and do not commit transaction or audit records.
- Financial operations create transaction history and audit-log records.

The financial service build has been verified locally. Live SQL Server execution and automated transaction tests remain pending until SQL Server/LocalDB is available.

## Limitations

This MVP has no authentication or authorization, uses a development CORS allow-list, and does not connect to real accounts, payment networks, or customer data. Add identity, authorization policies, operational monitoring, and a production deployment review before considering any non-demo use.
