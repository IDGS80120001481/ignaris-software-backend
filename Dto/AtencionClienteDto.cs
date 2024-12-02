using LignarisBack.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace LignarisBack.Dto
{
    public class AtencionClienteDto
    {
        public int IdAtencionCliente { get; set; }
        public string IdCliente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? Descripcion { get; set; }
        public string? TipoAtencion { get; set; }
        public string? Prioridad { get; set; }

        public string? Nombre { get; set; }
    }
}
