using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;

namespace Estacionamento.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Preco> Precos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<ConfiguracaoEstacionamento> Configuracoes { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveColumnType("timestamp without time zone");
    }

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

        modelBuilder.Entity<Preco>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorDiaria).HasPrecision(18, 2);
            entity.Property(e => e.DescontoPixDinheiro).HasPrecision(18, 2);
            entity.Property(e => e.TipoVaga).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NomeCliente).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TelefoneCliente).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CpfCliente).HasMaxLength(14);
            entity.Property(e => e.PlacaVeiculo).HasMaxLength(10);
            entity.Property(e => e.ValorDiaria).HasPrecision(18, 2);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
            entity.Property(e => e.DescontoAplicado).HasPrecision(18, 2);
            entity.Property(e => e.ValorFinal).HasPrecision(18, 2);
            entity.Property(e => e.TipoVaga).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.FormaPagamento).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Origem).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Observacoes).HasMaxLength(500);

            entity.HasIndex(e => e.DataEntrada);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<ConfiguracaoEstacionamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NomeEstacionamento).HasMaxLength(200);
            entity.Property(e => e.Endereco).HasMaxLength(300);
            entity.Property(e => e.Contato).HasMaxLength(20);
            entity.Property(e => e.Cnpj).HasMaxLength(20);
            entity.Property(e => e.TelefoneWhatsApp).HasMaxLength(20);
            entity.Property(e => e.MensagemWhatsApp).HasMaxLength(1000);
        });
    }
}
