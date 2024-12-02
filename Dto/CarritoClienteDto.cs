using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LignarisBack.Dto
{
    public class CarritoClienteDto
    {
        public int IdReceta { get; set; }
        public string? Nombre { get; set; }
        public string? Foto { get; set; }
        public int? Tamanio { get; set; }
        public double? PrecioUnitario { get; set; } 
        public int Cantidad { get; set; }
    
    }
}
