using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaMVC.Modelos.Entities;
public class PresupuestoDetalle
{
    [Key]
    public int Id { get; set; }

    public int PresupuestoId { get; set; }
    public int TipoGastoId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoPresupuestado { get; set; }

    // Foreign Keys
    [ForeignKey("PresupuestoId")]
    public virtual Presupuesto Presupuesto { get; set; }

    [ForeignKey("TipoGastoId")]
    public virtual TipoGasto TipoGasto { get; set; }
}
