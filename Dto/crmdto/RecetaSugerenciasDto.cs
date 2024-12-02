namespace LignarisBack.Dto.crmdto
{
    public class RecetaSugerenciasDto
    {
        public int IdReceta { get; set; }
        public string? Nombre { get; set; }
        public string? Foto { get; set; }
        public int? Tamanio { get; set; }
        public double? PrecioUnitario { get; set; }
        public int? Estatus { get; set; }
        public bool Suggest { get; set; }
        public List<RecetaDetalleSugerenciasDto>? RecetaDetalles { get; set; }
    }
}
