using LignarisBack.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LignarisBack.Dto
{
    public class CompaniaDto
    {
        public int IdCompania { get; set; }
        public string IdCliente { get; set; }
        public string? Nombre { get; set; }
        public string? RazonSocial { get; set; }
        public string? RFC { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
