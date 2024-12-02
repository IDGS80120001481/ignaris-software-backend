using LignarisBack.Models;
using Microsoft.AspNetCore.Identity;

namespace LignarisBack.Models
{
    public class AppUser : IdentityUser

    {
        public string? Fullname { get; set; }
        public DateTime LastSession { get; set; }
        public int FailedAttempts { get; set; }
        public int AccountActive { get; set; }

        public string TokenActivate { get; set; }
        public DateTime ExpirationTime { get; set; }

        public string Code { get; set; }
        public DateTime ExpirationCode { get; set; }

        public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

        public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

        public virtual ICollection<RegistroSesione> RegistroSesiones { get; set; } = new List<RegistroSesione>();
    }
}
