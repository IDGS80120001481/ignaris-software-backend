using LignarisBack.Controllers;
using LignarisBack.Models;
using System;
using System.Collections.Generic;

namespace LignarisBack.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public int IdPersona { get; set; }

    public string? IdUsuario { get; set; }
    public string? TelefonoFijo { get; set; }
    public string? Linkedin { get; set; }
    public string? Youtube { get; set; }
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? ComoLlego { get; set; }


    public virtual Persona IdPersonaNavigation { get; set; } = null!;

    public virtual AppUser IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
    public virtual ICollection<CarritoCompras> CarritoCompras { get; set; } = new List<CarritoCompras>();
    public virtual ICollection<Comentarios> Comentarios { get; set; } = new List<Comentarios>();
    public virtual ICollection<AtencionCliente> AtencionClientes { get; set; } = new List<AtencionCliente>();
    public virtual ICollection<ChatServer> ChatServer { get; set; } = new List<ChatServer>();
    public virtual Compania Compania { get; set; } = null!;
}
