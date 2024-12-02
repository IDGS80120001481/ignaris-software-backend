namespace LignarisBack.Dto
{
    public class HistorialVentaDto
    {
        public int IdVenta { get; set; }
        public DateOnly? FechaVenta { get; set; }

        public double? Total { get; set; }
        public string MetodoPago { get; set; }
        public string FullName { get; set; }

        public List<HistorialVentaDetalleDto> HistorialVentaDetalleDto { get; set; }
    }
}
