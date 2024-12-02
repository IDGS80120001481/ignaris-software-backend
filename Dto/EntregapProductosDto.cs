namespace LignarisBack.Dto
{
    public class EntregaProductosDto
    {
        public int? IdVenta { get; set; }

        public DateOnly? FechaVenta { get; set; }

        public double? Total { get; set; }
        public string Pagado { get; set; }
        public List<VentaDetalleProduccionDto>? DetalleVenta { get; set; }

        public PersonaDto? Persona { get; set; }
    }
}
