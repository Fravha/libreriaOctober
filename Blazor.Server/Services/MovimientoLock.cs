namespace Blazor.Server.Services;

// Serializa modificaciones de inventario entre solicitudes dentro de una sola instancia de la API.
// Para despliegue distribuido se requieren concurrencia y transacciones a nivel de BD.
public sealed class MovimientoLock
{
    public SemaphoreSlim Semaforo { get; } = new(1, 1);
}
