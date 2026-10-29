using Blazor.Server.Data;
using Blazor.Server.Models;
using Blazor.Server.Services;
using Blazor.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blazor.Server.Controllers;

[ApiController, Route("api/libros")]
public class LibrosController : ControllerBase
{
    private readonly LibreriaContext _db;
    private readonly MovimientoLock _locker;
    public LibrosController(LibreriaContext db, MovimientoLock locker)
    {
        _db = db; _locker = locker;
    }

    [HttpGet]
    public async Task<ActionResult<List<LibroDTO>>> Lista()
    {
        var libros = await _db.Libros.AsNoTracking().OrderBy(x => x.Titulo).ToListAsync();
        return Ok(libros.Select(x => x.ToDto()).ToList());
    }

    [HttpGet("categorias")]
    public ActionResult<string[]> Categorias() => Ok(CategoriasLibro.Todas);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LibroDTO>> Obtener(int id)
    {
        var libro = await _db.Libros.FindAsync(id);
        return libro is null ? NotFound() : Ok(libro.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<LibroDTO>> Crear(LibroGuardarDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo) || string.IsNullOrWhiteSpace(dto.Autor))
            return BadRequest("Título y autor son obligatorios.");
        if (!CategoriasLibro.Todas.Contains(dto.Categoria))
            return BadRequest("La categoría no pertenece al catálogo estático.");
        await _locker.Semaforo.WaitAsync();
        try
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            var libro = new Libro
            {
                Titulo = dto.Titulo.Trim(), Autor = dto.Autor.Trim(),
                Categoria = dto.Categoria, AnioPublicacion = dto.AnioPublicacion,
                PrecioVenta = dto.PrecioVenta, StockInicial = dto.StockInicial,
                StockActual = dto.StockInicial
            };
            _db.Libros.Add(libro);
            await _db.SaveChangesAsync();
            _db.MovimientosStock.Add(new MovimientoStock
            {
                IdLibro = libro.IdLibro, Tipo = "INICIAL", Cantidad = dto.StockInicial,
                StockResultante = libro.StockActual, Referencia = "Alta de libro"
            });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Created($"/api/libros/{libro.IdLibro}", libro.ToDto());
        }
        finally { _locker.Semaforo.Release(); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, LibroGuardarDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo) || string.IsNullOrWhiteSpace(dto.Autor))
            return BadRequest("Título y autor son obligatorios.");
        if (!CategoriasLibro.Todas.Contains(dto.Categoria))
            return BadRequest("La categoría no pertenece al catálogo estático.");
        var libro = await _db.Libros.FindAsync(id);
        if (libro is null) return NotFound();
        libro.Titulo = dto.Titulo.Trim();
        libro.Autor = dto.Autor.Trim();
        libro.Categoria = dto.Categoria;
        libro.AnioPublicacion = dto.AnioPublicacion;
        libro.PrecioVenta = dto.PrecioVenta;
        // No se altera StockInicial ni StockActual al editar la ficha: compras/ventas cambian existencias.
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var libro = await _db.Libros.FindAsync(id);
        if (libro is null) return NotFound();
        libro.Activo = false; // Borrado lógico: preserva compras, ventas y trazabilidad.
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
