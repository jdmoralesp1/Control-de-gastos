using PruebaTecnicaMVC.Utilidades;
using System.ComponentModel.DataAnnotations;

namespace PruebaTecnicaMVC.Modelos.Entities;
public class TipoGasto
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    public string Codigo { get; set; } = "TG";

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [StringLength(500)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = TimeUtil.GetDateTimeColombia();

    [Required]
    public Guid UsuarioId { get; set; }

    // Navegacion
    public virtual ICollection<PresupuestoDetalle> Presupuestos { get; set; } = new List<PresupuestoDetalle>();
    public virtual ICollection<GastoDetalle> GastoDetalles { get; set; } = new List<GastoDetalle>();
}
