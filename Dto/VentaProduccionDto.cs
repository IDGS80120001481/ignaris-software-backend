namespace LignarisBack.Dto
{
    public class VentaProduccionDto
    {
        public int? IdVenta { get; set; }

        public DateOnly? FechaVenta { get; set; }

        public double? Total { get; set; }
        public List<VentaDetalleProduccionDto>? DetalleVenta { get; set; }
       

    }
}
