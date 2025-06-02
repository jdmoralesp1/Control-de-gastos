namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct FondoMonetarioDto
(
    int Id,
    string Nombre,
    string Tipo,
    string? NumeroCuenta,
    decimal SaldoActual,
    string SaldoActualFormateado,
    string FechaCreacion
);
