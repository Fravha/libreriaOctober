using System.ComponentModel.DataAnnotations;

namespace Blazor.Shared.DTOs;

public static class CategoriasLibro
{
    public static readonly string[] Todas =
    {
        "Literatura", "Académico", "Infantil", "Historia",
        "Ciencia", "Religión", "Arte", "Otros"
    };
}

public class LibroDTO
{
    public int IdLibro { get; set; }
    public string Titulo { get; set; } = "";
    public string Autor { get; set; } = "";
    public string Categoria { get; set; } = "Otros";
    public short? AnioPublicacion { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockInicial { get; set; }
    public int StockActual { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; }
}

public class LibroGuardarDTO
{
    [Required, StringLength(180)]
    public string Titulo { get; set; } = "";
    [Required, StringLength(160)]
    public string Autor { get; set; } = "";
    [Required]
    public string Categoria { get; set; } = "Literatura";
    [Range(1000, 2100)]
    public short? AnioPublicacion { get; set; }
    [Range(0, 9999999)]
    public decimal PrecioVenta { get; set; }
    [Range(0, int.MaxValue)]
    public int StockInicial { get; set; }
}
