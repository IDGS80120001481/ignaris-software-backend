using LignarisBack.Models;
using LignarisBack.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniaController : ControllerBase
    {

        private readonly LignarisPizzaContext _basedatos;

        public CompaniaController(LignarisPizzaContext basedatos)
        {
            _basedatos = basedatos;
        }

        [HttpPost]
        [Route("add")]
        public async Task<ActionResult> InsertCompania([FromBody] CompaniaDto companiaDto)
        {
            var cliente = await _basedatos.Clientes
                                          .FirstOrDefaultAsync(c => c.IdUsuario == companiaDto.IdCliente);

            if (cliente == null)
            {
                return NotFound(new { result = "Cliente no encontrado." });
            }

            // Crear una nueva instancia de Compania
            var nuevaCompania = new Compania
            {
                IdCliente = cliente.IdCliente,
                Nombre = companiaDto.Nombre,
                RazonSocial = companiaDto.RazonSocial,
                RFC = companiaDto.RFC,
                Direccion = companiaDto.Direccion,
                Telefono = companiaDto.Telefono,
                Email = companiaDto.Email,
                FechaRegistro = DateTime.UtcNow
            };

            // Agregar la nueva compañía a la base de datos
            _basedatos.Compania.Add(nuevaCompania);
            await _basedatos.SaveChangesAsync();

            return Ok(new { result = "Compañía registrada exitosamente." });
        }

        [HttpGet]
        public async Task<ActionResult<List<CompaniaDto>>> GetCompanias()
        {
            var companias = await _basedatos.Compania
                                            .Select(c => new CompaniaDto
                                            {
                                                IdCompania = c.IdCompania,
                                                IdCliente = c.IdCliente.ToString(),
                                                Nombre = c.Nombre,
                                                RazonSocial = c.RazonSocial,
                                                RFC = c.RFC,
                                                Direccion = c.Direccion,
                                                Telefono = c.Telefono,
                                                Email = c.Email,
                                                FechaRegistro = c.FechaRegistro
                                            })
                                            .ToListAsync();
            return Ok(companias);
        }

    }
}
