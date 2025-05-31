namespace PruebaTecnicaMVC.Modelos.DTOs;
public record struct TipoGastoDto
(
    int Id,
    string Codigo,
    string Nombre,
    string? Descripcion,
    string FechaCreacion   
);
