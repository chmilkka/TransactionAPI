using Microsoft.EntityFrameworkCore;
using TransactionAPI.Models;

namespace TransactionAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Id)
                .HasColumnName("transaction_id")
                .HasColumnType("varchar");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Name)
                .HasColumnName("name")
                .HasColumnType("varchar");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Email)
                .HasColumnName("email")
                .HasColumnType("varchar");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransactionDate)
                .HasColumnName("transaction_date")
                .HasColumnType("timestamptz");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.ClientTimezone)
                .HasColumnName("client_timezone")
                .HasColumnType("varchar");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.ClientLocation)
                .HasColumnName("client_location")
                .HasColumnType("varchar");
        }
    }
}
