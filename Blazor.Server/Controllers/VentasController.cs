using Blazor.Server.Data;
using Blazor.Server.Models;
using Blazor.Server.Services;
using Blazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blazor.Server.Controllers;

[ApiController, Route("api/ventas")]
public class VentasController : ControllerBase
{
    private readonly LibreriaContext _db;
    private readonly MovimientoLock _locker;
    public VentasController(LibreriaContext db, MovimientoLock locker) { _db = db; _locker = locker; }

    [HttpGet]
    public async Task<ActionResult<List<VentaDTO>>> Lista()
    {
        var ventas = await _db.Ventas.AsNoTracking()
            .Include(x => x.Detalles).ThenInclude(x => x.Libro)
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.IdVenta)
            .ToListAsync();
        return Ok(ventas.Select(x => x.ToDto()).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<VentaDTO>> Registrar(VentaCrearDTO dto)
    {
        if (dto.Detalles is null || dto.Detalles.Count == 0)
            return BadRequest("Agrega al menos un libro a la venta.");
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
            foreach (var item in dto.Detalles)
                if (libros[item.IdLibro].StockActual < item.Cantidad)
                    return BadRequest($"Stock insuficiente de '{libros[item.IdLibro].Titulo}'. Disponible: {libros[item.IdLibro].StockActual}.");
            var venta = new Venta
            {
                NombreCliente = string.IsNullOrWhiteSpace(dto.NombreCliente) ? null : dto.NombreCliente.Trim(),
                NitCi = string.IsNullOrWhiteSpace(dto.NitCi) ? null : dto.NitCi.Trim(),
                Total = dto.Detalles.Sum(x => x.Cantidad * x.PrecioUnitario)
            };
            foreach (var item in dto.Detalles)
                venta.Detalles.Add(new DetalleVenta
                {
                    IdLibro = item.IdLibro, Cantidad = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                });
            _db.Ventas.Add(venta);
            await _db.SaveChangesAsync();
            foreach (var item in dto.Detalles)
            {
                var libro = libros[item.IdLibro];
                libro.StockActual -= item.Cantidad;
                _db.MovimientosStock.Add(new MovimientoStock
                {
                    IdLibro = item.IdLibro, Tipo = "VENTA", Cantidad = -item.Cantidad,
                    StockResultante = libro.StockActual, Referencia = $"Venta #{venta.IdVenta}"
                });
            }
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            foreach (var detalle in venta.Detalles) detalle.Libro = libros[detalle.IdLibro];
            return Ok(venta.ToDto());
        }
        finally { _locker.Semaforo.Release(); }
    }
}
