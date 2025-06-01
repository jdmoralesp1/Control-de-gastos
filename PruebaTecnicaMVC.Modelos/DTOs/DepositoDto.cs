namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct DepositoDto
(
    int Id,
    DateTime Fecha,
    string FondoMonetario,
    string Monto,
    string? Observaciones
);