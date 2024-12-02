using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LignarisBack.Models
{
    public class PersonalizedOffer
    {
        [Key]
        public int IdPersonalizedOffer { get; set; }
        public int idRecetum { get; set; }
        public int CantidadPizzas { get; set; }
        public double CantidadDinero { get; set; }
        public DateTime InicioOferta { get; set; }
        public DateTime FinOferta { get; set; }


        [ForeignKey("idRecetum")]
        public virtual Recetum Recetum { get; set; } = null!;
    }
}
