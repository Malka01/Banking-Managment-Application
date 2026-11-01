using BankCore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BankCore.Api.Data;

public class BankCoreDbContext(DbContextOptions<BankCoreDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Nic).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.Nic).IsUnique();
            entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
            entity.Property(x => x.PhoneNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Address).HasMaxLength(300).IsRequired();
        });
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccountNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.AccountNumber).IsUnique();
            entity.Property(x => x.Balance).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.ToTable("Accounts", table => table.HasCheckConstraint("CK_Accounts_Balance_NonNegative", "[Balance] >= 0"));
            entity.HasOne(x => x.Customer).WithMany(x => x.Accounts).HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReferenceNumber).HasMaxLength(40).IsRequired();
            entity.HasIndex(x => x.ReferenceNumber);
            entity.Property(x => x.Type).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.PreviousBalance).HasPrecision(18, 2);
            entity.Property(x => x.NewBalance).HasPrecision(18, 2);
            entity.ToTable("Transactions", table =>
                table.HasCheckConstraint("CK_Transactions_Amount_Positive", "[Amount] > 0"));
            entity.HasIndex(x => new { x.AccountId, x.CreatedAt });
            entity.HasOne(x => x.Account).WithMany(x => x.Transactions).HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ReferenceNumber).HasMaxLength(40);
            entity.Property(x => x.Details).HasMaxLength(500);
        });
    }
}
