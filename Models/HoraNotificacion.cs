namespace LignarisBack.Models
{
    public class HoraNotificacion
    {
        public int Id { get; set; }
        public TimeSpan HoraCarrito { get; set; }
        public bool ActivateCarrito { get; set; }
        public TimeSpan HoraSugerencias { get; set; }
        public bool ActivateSugerencias { get; set; }
    }
}
