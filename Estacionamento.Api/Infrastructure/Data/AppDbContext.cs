using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;

namespace Estacionamento.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Preco> Precos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<ConfiguracaoEstacionamento> Configuracoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Usuario).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.HasIndex(e => e.Usuario).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Cpf).HasMaxLength(14);
            entity.Property(e => e.PlacaVeiculo).HasMaxLength(10);
            entity.Property(e => e.ModeloVeiculo).HasMaxLength(50);
            entity.Property(e => e.CorVeiculo).HasMaxLength(30);
        });

        modelBuilder.Entity<Preco>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorDiaria).HasPrecision(18, 2);
            entity.Property(e => e.DescontoPix).HasPrecision(5, 2);
            entity.Property(e => e.TipoVaga).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorDiaria).HasPrecision(18, 2);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
            entity.Property(e => e.DescontoAplicado).HasPrecision(18, 2);
            entity.Property(e => e.ValorFinal).HasPrecision(18, 2);
            entity.Property(e => e.TipoVaga).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.FormaPagamento).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Origem).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Observacoes).HasMaxLength(500);

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.Reservas)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.DataReserva);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Pagamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).HasPrecision(18, 2);
            entity.Property(e => e.FormaPagamento).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Comprovante).HasMaxLength(255);
            entity.Property(e => e.Observacao).HasMaxLength(500);

            entity.HasOne(e => e.Reserva)
                  .WithMany(r => r.Pagamentos)
                  .HasForeignKey(e => e.ReservaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConfiguracaoEstacionamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TelefoneWhatsApp).HasMaxLength(20);
            entity.Property(e => e.MensagemWhatsApp).HasMaxLength(1000);
        });
    }
}
