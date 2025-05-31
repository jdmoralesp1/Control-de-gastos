using PruebaTecnicaMVC.Utilidades;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PruebaTecnicaMVC.Modelos.Entities;
public class FondoMonetario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; }

    [StringLength(50)]
    public string Tipo { get; set; } // "Banco", "Caja", "Efectivo"

    [StringLength(50)]
    public string? NumeroCuenta { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SaldoActual { get; set; } = 0;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = TimeUtil.GetDateTimeColombia();

    [Required]
    public Guid UsuarioId { get; set; }

    // Navegacion
    public virtual ICollection<Deposito> Depositos { get; set; } = new List<Deposito>();
    public virtual ICollection<GastoEncabezado> Gastos { get; set; } = new List<GastoEncabezado>();
}
