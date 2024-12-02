namespace LignarisBack.Dto
{
    public class ProductoVentasDTO
    {
        public string NombreReceta { get; set; }
        public int? IdReceta { get; set; }
        public decimal TotalCantidad { get; set; }
        public DateOnly FechaVenta { get; set; }
        public double TotalVentas { get; set; }
    }
}
