using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaMVC.Modelos.Entities;
public class GastoDetalle
{
    [Key]
    public int Id { get; set; }

    public int GastoEncabezadoId { get; set; }
    public int TipoGastoId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [StringLength(200)]
    [Required]
    public string Descripcion { get; set; }

    // Foreign Keys
    [ForeignKey("GastoEncabezadoId")]
    public virtual GastoEncabezado GastoEncabezado { get; set; }

    [ForeignKey("TipoGastoId")]
    public virtual TipoGasto TipoGasto { get; set; }
}
