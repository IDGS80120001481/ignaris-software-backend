using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LignarisBack.Models
{
    public class Comentarios
    {
        [Key]
        public int IdComentarios { get; set; }
        public int IdCliente { get; set; }
        public int IdReceta { get; set; }
        public string? TipoComentario { get; set; }
        public string? Comentario { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; } = null!;

        [ForeignKey("IdReceta")]
        public virtual Recetum Recetas { get; set; } = null!;
    }
}
