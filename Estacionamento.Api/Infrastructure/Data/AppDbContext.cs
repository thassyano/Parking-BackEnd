using Microsoft.EntityFrameworkCore;
using Estacionamento.Api.Domain.Entities;

namespace Estacionamento.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vaga> Vagas { get; set; }
    public DbSet<Ocupacao> Ocupacoes { get; set; }
    public DbSet<Preco> Precos { get; set; }
    public DbSet<Admin> Admins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de Vaga
        modelBuilder.Entity<Vaga>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Numero).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.Numero).IsUnique();
        });

        // Configuração de Ocupacao
        modelBuilder.Entity<Ocupacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlacaVeiculo).IsRequired().HasMaxLength(10);
            entity.HasOne(e => e.Vaga)
                  .WithMany(v => v.Ocupacoes)
                  .HasForeignKey(e => e.VagaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuração de Preco
        modelBuilder.Entity<Preco>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorHora).HasPrecision(18, 2);
            entity.Property(e => e.ValorMinuto).HasPrecision(18, 2);
        });

        // Configuração de Admin
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Usuario).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SenhaHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Usuario).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

