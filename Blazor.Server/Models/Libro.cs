using System.ComponentModel.DataAnnotations;

namespace Blazor.Server.Models;

public class Libro
{
    [Key] public int IdLibro { get; set; }
    [MaxLength(180)] public string Titulo { get; set; } = "";
    [MaxLength(160)] public string Autor { get; set; } = "";
    [MaxLength(60)] public string Categoria { get; set; } = "";
    public short? AnioPublicacion { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockInicial { get; set; }
    public int StockActual { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public List<DetalleCompra> DetallesCompra { get; set; } = new();
    public List<DetalleVenta> DetallesVenta { get; set; } = new();
}

public class Compra
{
    [Key] public int IdCompra { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    [MaxLength(160)] public string? Proveedor { get; set; }
    [MaxLength(300)] public string? Observacion { get; set; }
    public decimal Total { get; set; }
    public List<DetalleCompra> Detalles { get; set; } = new();
}

public class DetalleCompra
{
    [Key] public int IdDetalleCompra { get; set; }
    public int IdCompra { get; set; }
    public Compra Compra { get; set; } = null!;
    public int IdLibro { get; set; }
    public Libro Libro { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class Venta
{
    [Key] public int IdVenta { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    [MaxLength(160)] public string? NombreCliente { get; set; }
    [MaxLength(40)] public string? NitCi { get; set; }
    public decimal Total { get; set; }
    public List<DetalleVenta> Detalles { get; set; } = new();
}

public class DetalleVenta
{
    [Key] public int IdDetalleVenta { get; set; }
    public int IdVenta { get; set; }
    public Venta Venta { get; set; } = null!;
    public int IdLibro { get; set; }
    public Libro Libro { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class MovimientoStock
{
    [Key] public int IdMovimiento { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public int IdLibro { get; set; }
    public Libro Libro { get; set; } = null!;
    [MaxLength(20)] public string Tipo { get; set; } = "";
    public int Cantidad { get; set; } // Valor con signo: compra positivo, venta negativo.
    public int StockResultante { get; set; }
    [MaxLength(100)] public string Referencia { get; set; } = "";
}
