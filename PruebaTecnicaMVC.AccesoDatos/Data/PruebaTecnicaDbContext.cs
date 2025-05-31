using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PruebaTecnicaMVC.Modelos.Entities;

namespace PruebaTecnicaMVC.AccesoDatos.Data;

public class PruebaTecnicaDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public PruebaTecnicaDbContext(DbContextOptions<PruebaTecnicaDbContext> options) : base(options)
    {
    }

    public DbSet<Deposito> Depositos { get; set; }
    public DbSet<FondoMonetario> FondosMonetarios { get; set; }
    public DbSet<GastoDetalle> GastosDetalle { get; set; }
    public DbSet<GastoEncabezado> GastosEncabezado { get; set; }
    public DbSet<Presupuesto> Presupuestos { get; set; }
    public DbSet<PresupuestoDetalle> PresupuestosDetalle { get; set; }
    public DbSet<TipoGasto> TiposGasto { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones adicionales
        modelBuilder.Entity<PresupuestoDetalle>()
            .HasIndex(p => new { p.PresupuestoId, p.TipoGastoId })
            .IsUnique();

        modelBuilder.Entity<Presupuesto>()
            .HasIndex(p => new { p.Mes, p.Anio })
            .IsUnique();

        modelBuilder.Entity<TipoGasto>()
            .HasIndex(t => t.Codigo)
            .IsUnique();
    }
}
