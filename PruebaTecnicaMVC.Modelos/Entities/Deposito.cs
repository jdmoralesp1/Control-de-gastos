using PruebaTecnicaMVC.Utilidades;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaMVC.Modelos.Entities;
public class Deposito
{
    [Key]
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public int FondoMonetarioId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    public DateTime FechaCreacion { get; set; } = TimeUtil.GetDateTimeColombia();

    [Required]
    public Guid UsuarioId { get; set; }

    // Foreign Key
    [ForeignKey("FondoMonetarioId")]
    public virtual FondoMonetario FondoMonetario { get; set; }
}
