using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AtencionClienteController : ControllerBase
    {
        private readonly LignarisPizzaContext _basedatos;

        public AtencionClienteController(LignarisPizzaContext basedatos)
        {
            _basedatos = basedatos;
        }

        [HttpPost("insert")]
        public async Task<IActionResult> CreateAtencionCliente([FromBody] AtencionClienteDto atencion)
        {
            var cliente = await _basedatos.Clientes
                                          .Where(c => c.IdUsuario == atencion.IdCliente)
                                          .FirstOrDefaultAsync();

            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var nuevaAtencion = new AtencionCliente
            {
                IdCliente = cliente.IdCliente,
                Descripcion = atencion.Descripcion,
                TipoAtencion = atencion.TipoAtencion,
                Prioridad = atencion.Prioridad
            };

            _basedatos.AtencionCliente.Add(nuevaAtencion);

            try
            {
                await _basedatos.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al guardar los datos: {ex.Message}");
            }
            return Ok(new { result = "Se ha realizado el registro de la atencion de cliente." });
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<AtencionClienteDto>>> GetAllAtencionesCliente()
        {
            var atenciones = await _basedatos.AtencionCliente
                                             .Include(ac => ac.Cliente)
                                             .Take(150)
                                             .ToListAsync();

            if (atenciones == null || !atenciones.Any())
            {
                return NotFound("{\"result\": \"No se encontraron atenciones al cliente.\"}");
            }

            var clientesIds = atenciones.Select(c => c.IdCliente).ToList();
            var clientes = await _basedatos.Clientes
                .Where(c => clientesIds.Contains(c.IdCliente))
                .AsNoTracking()
                .ToListAsync();

            var usuariosIds = clientes.Select(c => c.IdUsuario).ToList();
            var usuarios = await _basedatos.AppUser
                .Where(a => usuariosIds.Contains(a.Id))
                .AsNoTracking()
                .ToListAsync();

            var atencionesDto = atenciones.Select(ac =>
            {
                var cliente = clientes.FirstOrDefault(c => c.IdCliente == ac.IdCliente);
                var usuario = usuarios.FirstOrDefault(u => u.Id == cliente?.IdUsuario);

                return new AtencionClienteDto
                {
                    IdAtencionCliente = ac.IdAtencionCliente,
                    FechaCreacion = ac.FechaCreacion,
                    Descripcion = ac.Descripcion,
                    TipoAtencion = ac.TipoAtencion,
                    Prioridad = ac.Prioridad,
                    Nombre = usuario.Fullname
                };
            }).ToList();
            return Ok(atencionesDto);
        }

        [HttpGet("{idCliente}")]
        public async Task<ActionResult<IEnumerable<AtencionClienteDto>>> GetAtencionesPorCliente(int idCliente)
        {
            var cliente = await _basedatos.Clientes
                .Where(c => c.IdPersona == idCliente)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var atenciones = await _basedatos.AtencionCliente
                                             .Where(ac => ac.IdCliente == cliente.IdCliente)
                                             .Include(ac => ac.Cliente)
                                             .ToListAsync();

            if (atenciones == null || !atenciones.Any())
            {
                return NotFound("{\"result\": \"No se encontraron atenciones para el cliente especificado.\"}");
            }
            var usuario = await _basedatos.AppUser
                .Where(a => a.Id == cliente.IdUsuario)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            var atencionesDto = atenciones.Select(ac => new AtencionClienteDto
            {
                IdAtencionCliente = ac.IdAtencionCliente,
                FechaCreacion = ac.FechaCreacion,
                Descripcion = ac.Descripcion,
                TipoAtencion = ac.TipoAtencion,
                Prioridad = ac.Prioridad,
                Nombre = usuario?.Fullname
            }).ToList();

            return Ok(atencionesDto);
        }

    }
}
