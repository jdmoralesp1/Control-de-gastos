namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct DepositoDto
(
    int Id,
    DateTime Fecha,
    string FondoMonetario,
    decimal Monto,
    string MontoFormateado,
    string? Observaciones
);