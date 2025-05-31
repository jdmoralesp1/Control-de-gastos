namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct DepositoDto
(
    int Id,
    string Fecha,
    string FondoMonetario,
    string Monto,
    string? Observaciones
);