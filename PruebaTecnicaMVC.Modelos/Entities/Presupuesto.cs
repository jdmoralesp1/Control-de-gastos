using PruebaTecnicaMVC.Utilidades;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaMVC.Modelos.Entities;
public class Presupuesto
{
    [Key]
    public int Id { get; set; }

    public int Mes { get; set; }
    public int Anio { get; set; }

    [Required]
    public Guid UsuarioId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    public DateTime FechaCreacion { get; set; } = TimeUtil.GetDateTimeColombia();

    public bool Activo { get; set; } = true;

    // Navegacion
    public virtual ICollection<PresupuestoDetalle> Detalles { get; set; } = new List<PresupuestoDetalle>();
}