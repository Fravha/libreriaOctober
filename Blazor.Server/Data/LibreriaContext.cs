using Blazor.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Blazor.Server.Data;

public class LibreriaContext : DbContext
{
    public LibreriaContext(DbContextOptions<LibreriaContext> options) : base(options) { }

    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompra => Set<DetalleCompra>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Libro>().Property(x => x.PrecioVenta).HasPrecision(18, 2);
        modelBuilder.Entity<Compra>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<Venta>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleCompra>().Property(x => x.PrecioUnitario).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleVenta>().Property(x => x.PrecioUnitario).HasPrecision(18, 2);
        modelBuilder.Entity<Libro>().HasIndex(x => x.Categoria);
        modelBuilder.Entity<Libro>().HasIndex(x => x.Titulo);
        modelBuilder.Entity<DetalleCompra>().HasOne(x => x.Libro).WithMany(x => x.DetallesCompra).HasForeignKey(x => x.IdLibro).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DetalleVenta>().HasOne(x => x.Libro).WithMany(x => x.DetallesVenta).HasForeignKey(x => x.IdLibro).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MovimientoStock>().HasOne(x => x.Libro).WithMany().HasForeignKey(x => x.IdLibro).OnDelete(DeleteBehavior.Restrict);
    }
}
