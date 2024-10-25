using Microsoft.EntityFrameworkCore;
using Wallet.Domain.Entities.Models;

namespace Wallet.Infrastructure.Context
{
    public class WalletDbContext : DbContext
    {
        public WalletDbContext(DbContextOptions<WalletDbContext> options)
            : base(options)
        {
        }

        public DbSet<Domain.Entities.Models.Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Domain.Entities.Models.Wallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Mnemonic).IsRequired();
                entity.HasMany(e => e.Addresses)
                      .WithOne()
                      .HasForeignKey(e => e.WalletId);
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AddressValue).IsRequired();
                entity.Property(e => e.IsUsed).IsRequired();
                entity.HasMany(e => e.Transactions)
                      .WithOne()
                      .HasForeignKey(e => e.AddressId);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Timestamp);
                entity.Property(e => e.SenderAddress).IsRequired();
                entity.Property(e => e.RecipientAddress).IsRequired();
            });
        }
    }
}
