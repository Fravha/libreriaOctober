using System.ComponentModel.DataAnnotations;

namespace Blazor.Shared.DTOs;

public class DetalleOperacionInput
{
    [Range(1, int.MaxValue)]
    public int IdLibro { get; set; }
    [Range(1, 1000000)]
    public int Cantidad { get; set; } = 1;
    [Range(0, 9999999)]
    public decimal PrecioUnitario { get; set; }
}

public class CompraCrearDTO
{
    [StringLength(160)]
    public string? Proveedor { get; set; }
    [StringLength(300)]
    public string? Observacion { get; set; }
    [MinLength(1)]
    public List<DetalleOperacionInput> Detalles { get; set; } = new();
}

public class VentaCrearDTO
{
    [StringLength(160)]
    public string? NombreCliente { get; set; }
    [StringLength(40)]
    public string? NitCi { get; set; }
    [MinLength(1)]
    public List<DetalleOperacionInput> Detalles { get; set; } = new();
}

public class DetalleOperacionDTO
{
    public int IdLibro { get; set; }
    public string Titulo { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

public class CompraDTO
{
    public int IdCompra { get; set; }
    public DateTime Fecha { get; set; }
    public string? Proveedor { get; set; }
    public string? Observacion { get; set; }
    public decimal Total { get; set; }
    public List<DetalleOperacionDTO> Detalles { get; set; } = new();
}

public class VentaDTO
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public string? NombreCliente { get; set; }
    public string? NitCi { get; set; }
    public string ClienteMostrar => string.IsNullOrWhiteSpace(NombreCliente) ? "Venta general" : NombreCliente;
    public decimal Total { get; set; }
    public List<DetalleOperacionDTO> Detalles { get; set; } = new();
}

public class MovimientoStockDTO
{
    public int IdMovimiento { get; set; }
    public DateTime Fecha { get; set; }
    public int IdLibro { get; set; }
    public string Titulo { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int Cantidad { get; set; }
    public int StockResultante { get; set; }
    public string Referencia { get; set; } = "";
}

public class DashboardDTO
{
    public int TotalLibros { get; set; }
    public int UnidadesDisponibles { get; set; }
    public int LibrosStockBajo { get; set; }
    public decimal ValorInventario { get; set; }
    public decimal VentasTotales { get; set; }
    public int NumeroVentas { get; set; }
    public int NumeroCompras { get; set; }
    public List<VentaDTO> UltimasVentas { get; set; } = new();
    public List<LibroDTO> StockCritico { get; set; } = new();
}
