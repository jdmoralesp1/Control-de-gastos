using PruebaTecnicaMVC.Utilidades;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaMVC.Modelos.Entities;
public class GastoEncabezado
{
    [Key]
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public int FondoMonetarioId { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [Required]
    [StringLength(200)]
    public string NombreComercio { get; set; }

    [StringLength(50)]
    public string? TipoDocumento { get; set; } // "Comprobante", "Factura", "Otro"

    [StringLength(50)]
    public string? NumeroDocumento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    public DateTime FechaCreacion { get; set; } = TimeUtil.GetDateTimeColombia();

    // Foreign Key
    [ForeignKey("FondoMonetarioId")]
    public virtual FondoMonetario FondoMonetario { get; set; }

    [Required]
    public Guid UsuarioId { get; set; }

    // Navegacion
    public virtual ICollection<GastoDetalle> Detalles { get; set; } = new List<GastoDetalle>();
}
