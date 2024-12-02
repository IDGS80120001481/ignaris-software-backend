using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LignarisBack.Models
{
    public class AtencionCliente
    {
        [Key]
        public int IdAtencionCliente { get; set; }
        public int IdCliente { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public string? Descripcion { get; set; }
        public string? TipoAtencion { get; set; }
        public string? Prioridad { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; } = null!;
    }
}
