namespace LignarisBack.Dto
{
    public class VentaEmpleadoDTO
    {
        public string NombreEmpleado { get; set; }
        public int IdVenta { get; set; }
        public decimal Cantidad { get; set; }
        public DateOnly FechaVenta { get; set; }
        public double Total { get; set; }
    }

}
