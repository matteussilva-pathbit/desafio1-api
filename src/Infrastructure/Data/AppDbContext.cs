using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Nome).IsRequired().HasMaxLength(200);
                b.Property(x => x.Preco).HasColumnType("numeric(18,2)");
                b.Property(x => x.QuantityAvailable).IsRequired();
            });

            modelBuilder.Entity<Customer>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Nome).IsRequired().HasMaxLength(200);
                b.Property(x => x.Email).IsRequired().HasMaxLength(200);
            });

            modelBuilder.Entity<Order>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.CepEntrega).IsRequired().HasMaxLength(20);
                b.Property(x => x.EnderecoEntrega).IsRequired().HasMaxLength(400);
                b.Property(x => x.PrecoUnitario).HasColumnType("numeric(18,2)");

                b.HasOne(x => x.Customer)
                 .WithMany(c => c.Orders)
                 .HasForeignKey(x => x.CustomerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(x => x.Product)
                 .WithMany()
                 .HasForeignKey(x => x.ProductId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Email).IsRequired().HasMaxLength(200);
                b.Property(x => x.Username).IsRequired().HasMaxLength(100);
                b.Property(x => x.PasswordHash).IsRequired();
            });
        }
    }
}
