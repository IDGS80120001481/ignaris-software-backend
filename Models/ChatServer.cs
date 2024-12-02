using System.ComponentModel.DataAnnotations.Schema;

namespace LignarisBack.Models
{
    public class ChatServer
    {
        public int Id { get; set; } 
        public int IdCliente { get; set; }
        public int IdEmpleado { get; set; }
        public string? Message { get; set; }
        public string Send { get; set; }
        public bool View { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; } = null!;
        [ForeignKey("IdEmpleado")]
        public virtual Empleado Empleado { get; set; } = null!;
    }
}
