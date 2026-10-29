using Blazor.Server.Data;
using Blazor.Server.Services;
using Blazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blazor.Server.Controllers;

[ApiController, Route("api/inventario")]
public class InventarioController : ControllerBase
{
    private readonly LibreriaContext _db;
    public InventarioController(LibreriaContext db) { _db = db; }

    [HttpGet]
    public async Task<ActionResult<List<LibroDTO>>> Listar([FromQuery] string? categoria)
    {
        var query = _db.Libros.AsNoTracking().Where(x => x.Activo);
        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(x => x.Categoria == categoria);
        var libros = await query.OrderBy(x => x.Titulo).ToListAsync();
        return Ok(libros.Select(x => x.ToDto()));
    }

    [HttpGet("movimientos")]
    public async Task<ActionResult<List<MovimientoStockDTO>>> Movimientos([FromQuery] int? idLibro)
    {
        var query = _db.MovimientosStock.AsNoTracking().Include(x => x.Libro).AsQueryable();
        if (idLibro.HasValue) query = query.Where(x => x.IdLibro == idLibro.Value);
        var movimientos = await query.OrderByDescending(x => x.IdMovimiento).Take(150)
            .Select(x => new MovimientoStockDTO
            {
                IdMovimiento = x.IdMovimiento, Fecha = x.Fecha, IdLibro = x.IdLibro,
                Titulo = x.Libro.Titulo, Tipo = x.Tipo, Cantidad = x.Cantidad,
                StockResultante = x.StockResultante, Referencia = x.Referencia
            }).ToListAsync();
        return Ok(movimientos);
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<DashboardDTO>> Resumen()
    {
        var libros = await _db.Libros.AsNoTracking().Where(x => x.Activo).ToListAsync();
        var ventas = await _db.Ventas.AsNoTracking()
            .Include(x => x.Detalles).ThenInclude(x => x.Libro)
            .OrderByDescending(x => x.IdVenta).ToListAsync();
        var resumen = new DashboardDTO
        {
            TotalLibros = libros.Count,
            UnidadesDisponibles = libros.Sum(x => x.StockActual),
            LibrosStockBajo = libros.Count(x => x.StockActual <= 5),
            ValorInventario = libros.Sum(x => x.StockActual * x.PrecioVenta),
            VentasTotales = ventas.Sum(x => x.Total),
            NumeroVentas = ventas.Count,
            NumeroCompras = await _db.Compras.CountAsync(),
            UltimasVentas = ventas.Take(5).Select(x => x.ToDto()).ToList(),
            StockCritico = libros.Where(x => x.StockActual <= 5).OrderBy(x => x.StockActual).Take(6).Select(x => x.ToDto()).ToList()
        };
        return Ok(resumen);
    }
}
