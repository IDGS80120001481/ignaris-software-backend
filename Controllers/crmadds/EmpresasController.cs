using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers.crmadds
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresasController : ControllerBase
    {
        private readonly LignarisPizzaContext _context;

        public EmpresasController(LignarisPizzaContext context)
        {
            _context = context;
        }

        [HttpPost("InsertarCompania")]
        public async Task<ActionResult> InsertarCompania([FromBody] CompaniaDto companiaDto)
        {
            if (companiaDto == null)
            {
                return BadRequest("{\"result\": \"Datos de la compañía no proporcionados.\"}");
            }

            var cliente = await _context.Clientes
                                         .Where(c => c.IdUsuario!.Contains(companiaDto.IdCliente))
                                         .FirstOrDefaultAsync();

            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var compania = new Compania
            {
                IdCliente = cliente.IdCliente,
                Nombre = companiaDto.Nombre,
                RazonSocial = companiaDto.RazonSocial,
                RFC = companiaDto.RFC,
                Direccion = companiaDto.Direccion,
                Telefono = companiaDto.Telefono,
                Email = companiaDto.Email,
                FechaRegistro = DateTime.UtcNow,
                Cliente = cliente
            };

            _context.Compania.Add(compania);

            await _context.SaveChangesAsync();

            return Ok("{\"result\": \"Compañía insertada exitosamente.\"}");
        }

        [HttpGet("ObtenerCompanias")]
        public async Task<ActionResult> ObtenerCompanias()
        {
            var companias = await _context.Compania
                                            .Include(c => c.Cliente)
                                            .Select(c => new
                                            {
                                                c.IdCompania,
                                                c.Nombre,
                                                c.RazonSocial,
                                                c.RFC,
                                                c.Direccion,
                                                c.Telefono,
                                                c.Email,
                                                c.FechaRegistro
                                            })
                                            .ToListAsync();

            if (companias == null || !companias.Any())
            {
                return NotFound("{\"result\": \"No se encontraron compañías registradas.\"}");
            }

            return Ok(companias);
        }

    }
}
