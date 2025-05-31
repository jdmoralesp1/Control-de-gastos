using PruebaTecnicaMVC.Modelos.Entities;

namespace PruebaTecnicaMVC.Modelos.DTOs;
public class GastoDto
{
    public GastoEncabezado GastoEncabezado { get; set; }
    public List<GastoDetalle> Detalle { get; set; }
}
