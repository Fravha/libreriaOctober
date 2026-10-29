using Blazor.Server.Models;
using Blazor.Shared.DTOs;

namespace Blazor.Server.Services;

public static class Mapper
{
    public static LibroDTO ToDto(this Libro x) => new()
    {
        IdLibro = x.IdLibro, Titulo = x.Titulo, Autor = x.Autor,
        Categoria = x.Categoria, AnioPublicacion = x.AnioPublicacion,
        PrecioVenta = x.PrecioVenta, StockInicial = x.StockInicial,
        StockActual = x.StockActual, Activo = x.Activo, FechaRegistro = x.FechaRegistro
    };
    public static CompraDTO ToDto(this Compra x) => new()
    {
        IdCompra = x.IdCompra, Fecha = x.Fecha, Proveedor = x.Proveedor,
        Observacion = x.Observacion, Total = x.Total,
        Detalles = x.Detalles.Select(d => new DetalleOperacionDTO
        {
            IdLibro = d.IdLibro, Titulo = d.Libro.Titulo,
            Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario
        }).ToList()
    };
    public static VentaDTO ToDto(this Venta x) => new()
    {
        IdVenta = x.IdVenta, Fecha = x.Fecha, NombreCliente = x.NombreCliente,
        NitCi = x.NitCi, Total = x.Total,
        Detalles = x.Detalles.Select(d => new DetalleOperacionDTO
        {
            IdLibro = d.IdLibro, Titulo = d.Libro.Titulo,
            Cantidad = d.Cantidad, PrecioUnitario = d.PrecioUnitario
        }).ToList()
    };
}
