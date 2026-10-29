using Blazor.Server.Data;
using Blazor.Server.Models;
using Blazor.Server.Services;
using Blazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blazor.Server.Controllers;

[ApiController, Route("api/compras")]
public class ComprasController : ControllerBase
{
    private readonly LibreriaContext _db;
    private readonly MovimientoLock _locker;
    public ComprasController(LibreriaContext db, MovimientoLock locker) { _db = db; _locker = locker; }

    [HttpGet]
    public async Task<ActionResult<List<CompraDTO>>> Lista()
    {
        var compras = await _db.Compras.AsNoTracking()
            .Include(x => x.Detalles).ThenInclude(x => x.Libro)
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.IdCompra)
            .ToListAsync();
        return Ok(compras.Select(x => x.ToDto()).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CompraDTO>> Registrar(CompraCrearDTO dto)
    {
        if (dto.Detalles is null || dto.Detalles.Count == 0)
            return BadRequest("Agrega al menos un libro a la compra.");
        if (dto.Detalles.Any(x => x.IdLibro <= 0 || x.Cantidad <= 0 || x.PrecioUnitario < 0))
            return BadRequest("Los ítems deben tener libro, cantidad positiva y precio no negativo.");
        if (dto.Detalles.GroupBy(x => x.IdLibro).Any(x => x.Count() > 1))
            return BadRequest("Agrupa los ítems repetidos del mismo libro.");
        await _locker.Semaforo.WaitAsync();
        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            var ids = dto.Detalles.Select(x => x.IdLibro).ToList();
            var libros = await _db.Libros.Where(x => ids.Contains(x.IdLibro) && x.Activo)
                .ToDictionaryAsync(x => x.IdLibro);
            if (libros.Count != ids.Count)
                return BadRequest("Uno o más libros no existen o están inactivos.");
            if (dto.Detalles.Any(x => (long)libros[x.IdLibro].StockActual + x.Cantidad > int.MaxValue))
                return BadRequest("La cantidad supera el límite de inventario.");
            var compra = new Compra
            {
                Proveedor = string.IsNullOrWhiteSpace(dto.Proveedor) ? null : dto.Proveedor.Trim(),
                Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim(),
                Total = dto.Detalles.Sum(x => x.Cantidad * x.PrecioUnitario)
            };
            foreach (var item in dto.Detalles)
                compra.Detalles.Add(new DetalleCompra
                {
                    IdLibro = item.IdLibro, Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                });
            _db.Compras.Add(compra);
            await _db.SaveChangesAsync();
            foreach (var item in dto.Detalles)
            {
                var libro = libros[item.IdLibro];
                libro.StockActual += item.Cantidad;
                _db.MovimientosStock.Add(new MovimientoStock
                {
                    IdLibro = item.IdLibro, Tipo = "COMPRA", Cantidad = item.Cantidad,
                    StockResultante = libro.StockActual, Referencia = $"Compra #{compra.IdCompra}"
                });
            }
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            // Navegaciones detalle-libro para la respuesta.
            foreach (var detalle in compra.Detalles) detalle.Libro = libros[detalle.IdLibro];
            return Ok(compra.ToDto());
        }
        finally { _locker.Semaforo.Release(); }
    }
}
