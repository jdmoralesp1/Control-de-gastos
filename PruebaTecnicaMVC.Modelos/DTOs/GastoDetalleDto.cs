namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct GastoDetalleDto
(
    int Id,
    string TipoGasto,
    string Monto,
    string Descripcion
);
