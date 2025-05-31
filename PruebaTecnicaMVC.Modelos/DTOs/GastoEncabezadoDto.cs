namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct GastoEncabezadoDto
(
    int Id,
    string Fecha,
    string FondoMonetario,
    string NombreComercio,
    string? TipoDocumento,
    string? NumeroDocumento,
    string MontoTotal,
    string? Observaciones
);
