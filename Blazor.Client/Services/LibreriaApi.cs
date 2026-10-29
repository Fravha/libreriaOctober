using System.Net.Http.Json;
using Blazor.Shared.DTOs;

namespace Blazor.Client.Services;

public class LibreriaApi
{
    private readonly HttpClient _http;
    public LibreriaApi(HttpClient http) { _http = http; }

    public async Task<List<LibroDTO>> Libros() =>
        await _http.GetFromJsonAsync<List<LibroDTO>>("api/libros") ?? new();
    public async Task<List<LibroDTO>> Inventario(string? categoria = null) =>
        await _http.GetFromJsonAsync<List<LibroDTO>>(
            "api/inventario" + (string.IsNullOrWhiteSpace(categoria) ? "" : "?categoria=" + Uri.EscapeDataString(categoria))) ?? new();
    public async Task<List<CompraDTO>> Compras() =>
        await _http.GetFromJsonAsync<List<CompraDTO>>("api/compras") ?? new();
    public async Task<List<VentaDTO>> Ventas() =>
        await _http.GetFromJsonAsync<List<VentaDTO>>("api/ventas") ?? new();
    public async Task<DashboardDTO> Resumen() =>
        await _http.GetFromJsonAsync<DashboardDTO>("api/inventario/resumen") ?? new();
    public async Task<List<MovimientoStockDTO>> Movimientos() =>
        await _http.GetFromJsonAsync<List<MovimientoStockDTO>>("api/inventario/movimientos") ?? new();

    public async Task<LibroDTO> CrearLibro(LibroGuardarDTO data) =>
        await Post<LibroDTO>("api/libros", data);
    public async Task EditarLibro(int id, LibroGuardarDTO data) =>
        await Enviar(_http.PutAsJsonAsync($"api/libros/{id}", data));
    public async Task DesactivarLibro(int id) =>
        await Enviar(_http.DeleteAsync($"api/libros/{id}"));
    public async Task<CompraDTO> CrearCompra(CompraCrearDTO data) =>
        await Post<CompraDTO>("api/compras", data);
    public async Task<VentaDTO> CrearVenta(VentaCrearDTO data) =>
        await Post<VentaDTO>("api/ventas", data);

    private async Task<T> Post<T>(string ruta, object data)
    {
        using var res = await _http.PostAsJsonAsync(ruta, data);
        await Validar(res);
        return await res.Content.ReadFromJsonAsync<T>() ?? throw new Exception("Respuesta vacía del servidor.");
    }
    private static async Task Enviar(Task<HttpResponseMessage> solicitud)
    {
        using var res = await solicitud;
        await Validar(res);
    }
    private static async Task Validar(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;
        var detalle = await res.Content.ReadAsStringAsync();
        throw new Exception(string.IsNullOrWhiteSpace(detalle)
            ? $"Error del servidor: {(int)res.StatusCode}"
            : detalle.Length > 500 ? detalle[..500] : detalle);
    }
}
