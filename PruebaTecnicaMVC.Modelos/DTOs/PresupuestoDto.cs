namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct PresupuestoDto
(
    int Id,
    string Mes,
    int Anio,
    string MontoTotal,
    string FechaCreacion
);
