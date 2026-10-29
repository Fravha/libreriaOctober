using Blazor.Server.Data;
using Blazor.Server.Models;

namespace Blazor.Server.Services;

public static class DemoSeeder
{
    public static void Seed(LibreriaContext db)
    {
        if (db.Libros.Any()) return;
        using var tx = db.Database.BeginTransaction();
        var libros = new[]
        {
            new Libro { Titulo = "El principito", Autor = "Antoine de Saint-Exupéry", Categoria = "Literatura", AnioPublicacion = 1943, PrecioVenta = 75, StockInicial = 18 },
            new Libro { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Literatura", AnioPublicacion = 1967, PrecioVenta = 135, StockInicial = 12 },
            new Libro { Titulo = "Álgebra de Baldor", Autor = "Aurelio Baldor", Categoria = "Académico", AnioPublicacion = 1941, PrecioVenta = 110, StockInicial = 15 },
            new Libro { Titulo = "El mundo de Sofía", Autor = "Jostein Gaarder", Categoria = "Infantil", AnioPublicacion = 1991, PrecioVenta = 95, StockInicial = 8 },
            new Libro { Titulo = "Breve historia del tiempo", Autor = "Stephen Hawking", Categoria = "Ciencia", AnioPublicacion = 1988, PrecioVenta = 125, StockInicial = 6 },
            new Libro { Titulo = "Historia de Bolivia", Autor = "Autores varios", Categoria = "Historia", PrecioVenta = 90, StockInicial = 4 },
            new Libro { Titulo = "Biblia de estudio", Autor = "Editorial San Pablo", Categoria = "Religión", PrecioVenta = 185, StockInicial = 9 },
            new Libro { Titulo = "Introducción al dibujo", Autor = "Editorial Creativa", Categoria = "Arte", PrecioVenta = 65, StockInicial = 3 }
        };
        foreach (var l in libros) l.StockActual = l.StockInicial;
        db.Libros.AddRange(libros);
        db.SaveChanges();
        db.MovimientosStock.AddRange(libros.Select(x => new MovimientoStock
        {
            IdLibro = x.IdLibro, Tipo = "INICIAL", Cantidad = x.StockInicial,
            StockResultante = x.StockActual, Referencia = "Carga inicial"
        }));
        db.SaveChanges();

        // Operaciones de ejemplo para que el dashboard muestre resultados desde el primer inicio.
        var compra = new Compra
        {
            Proveedor = "Distribuidora Editorial Central", Observacion = "Reposición de títulos",
            Total = 6 * 70m + 10 * 45m,
            Detalles = new List<DetalleCompra>
            {
                new() { IdLibro = libros[2].IdLibro, Cantidad = 6, PrecioUnitario = 70m },
                new() { IdLibro = libros[0].IdLibro, Cantidad = 10, PrecioUnitario = 45m }
            }
        };
        db.Compras.Add(compra);
        db.SaveChanges();
        foreach (var d in compra.Detalles)
        {
            var libro = libros.Single(x => x.IdLibro == d.IdLibro);
            libro.StockActual += d.Cantidad;
            db.MovimientosStock.Add(new MovimientoStock
            {
                IdLibro = d.IdLibro, Tipo = "COMPRA", Cantidad = d.Cantidad,
                StockResultante = libro.StockActual, Referencia = $"Compra #{compra.IdCompra}"
            });
        }
        db.SaveChanges();

        var ventas = new[]
        {
            new Venta
            {
                Total = 335m, Detalles = new List<DetalleVenta>
                {
                    new() { IdLibro = libros[0].IdLibro, Cantidad = 2, PrecioUnitario = 75m },
                    new() { IdLibro = libros[6].IdLibro, Cantidad = 1, PrecioUnitario = 185m }
                }
            },
            new Venta
            {
                NombreCliente = "María Fernández", NitCi = "1234567", Total = 270m,
                Detalles = new List<DetalleVenta>
                {
                    new() { IdLibro = libros[1].IdLibro, Cantidad = 2, PrecioUnitario = 135m }
                }
            },
            new Venta
            {
                NombreCliente = "Luis Vargas", Total = 110m,
                Detalles = new List<DetalleVenta>
                {
                    new() { IdLibro = libros[2].IdLibro, Cantidad = 1, PrecioUnitario = 110m }
                }
            }
        };
        foreach (var venta in ventas)
        {
            db.Ventas.Add(venta);
            db.SaveChanges();
            foreach (var d in venta.Detalles)
            {
                var libro = libros.Single(x => x.IdLibro == d.IdLibro);
                libro.StockActual -= d.Cantidad;
                db.MovimientosStock.Add(new MovimientoStock
                {
                    IdLibro = d.IdLibro, Tipo = "VENTA", Cantidad = -d.Cantidad,
                    StockResultante = libro.StockActual, Referencia = $"Venta #{venta.IdVenta}"
                });
            }
            db.SaveChanges();
        }
        tx.Commit();
    }
}
