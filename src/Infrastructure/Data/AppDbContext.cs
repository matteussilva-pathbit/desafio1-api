using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(x => x.Id);
                e.Property(x => x.Email).HasMaxLength(200).IsRequired();
                e.Property(x => x.UserName).HasMaxLength(100).IsRequired();
                e.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
                e.Property(x => x.Type).HasMaxLength(20).IsRequired();

                e.HasIndex(x => x.Email).IsUnique();
                e.HasIndex(x => x.UserName).IsUnique();
            });

            // CUSTOMER
            modelBuilder.Entity<Customer>(e =>
            {
                e.ToTable("Customers");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
                e.Property(x => x.Email).HasMaxLength(200).IsRequired();
                e.HasIndex(x => x.Email).IsUnique();
            });

            // PRODUCT
            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("Products");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nome).HasMaxLength(200).IsRequired();
                e.Property(x => x.Preco).HasColumnType("numeric(18,2)").IsRequired();
                e.Property(x => x.QuantityAvailable).IsRequired();
            });

            // ORDER  (mapeado aos nomes ATUAIS da entidade)
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("Orders");
                e.HasKey(x => x.Id);

                e.Property(x => x.Quantidade).IsRequired();
                e.Property(x => x.Preco).HasColumnType("numeric(18,2)").IsRequired();
                e.Property(x => x.EnderecoEntrega).HasMaxLength(500).IsRequired();
                e.Property(x => x.DataPedido).IsRequired();
                e.Property(x => x.Status).HasMaxLength(50).IsRequired();

                // Relacionamentos somente por FK, sem navegação na entidade
                e.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired()
                    .HasConstraintName("FK_Orders_Customers");

                e.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired()
                    .HasConstraintName("FK_Orders_Products");

                // Índices úteis
                e.HasIndex(x => x.CustomerId);
                e.HasIndex(x => x.ProductId);
                e.HasIndex(x => x.DataPedido);
            });
        }
    }
}
