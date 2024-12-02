using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LignarisBack.Models
{
    public class Compania
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCompania { get; set; }

        public int IdCliente { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; } = null!;

        public string? Nombre { get; set; }
        public string? RazonSocial { get; set; }
        public string? RFC { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
