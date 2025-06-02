namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct PresupuestoDto
(
    int Id,
    string Mes,
    int Anio,
    decimal MontoTotal,
    string MontoTotalFormateado,
    string FechaCreacion
);
