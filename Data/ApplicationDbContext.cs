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
                .HasColumnName("transaction_id");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Name)
                .HasColumnName("name");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Email)
                .HasColumnName("email");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransactionDate)
                .HasColumnName("transaction_date")
                .HasColumnType("datetime");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.ClientLocation)
                .HasColumnName("client_location");
        }
    }
}
