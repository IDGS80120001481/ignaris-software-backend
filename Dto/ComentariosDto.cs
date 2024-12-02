using System.ComponentModel.DataAnnotations.Schema;

namespace LignarisBack.Dto
{
    public class ComentariosDto
    {
        public string? IdComentarios { get; set; }
        public int idReceta { get; set; }
        public string? TipoComentario { get; set; }
        public string? Comentario { get; set; }

        public string? Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
