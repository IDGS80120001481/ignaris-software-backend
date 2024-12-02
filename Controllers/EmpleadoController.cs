using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Empleado")]
    [ApiController]
    public class EmpleadoController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly LignarisPizzaContext _basedatos;

        public EmpleadoController(UserManager<AppUser> userManager, RoleManager<IdentityRole>
       roleManager, IConfiguration configuration, LignarisPizzaContext basedatos)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _basedatos = basedatos;
        }

        [HttpGet]
        [Route("perfil")]
        [Authorize(Roles = "Cliente,Empleado")]
        public async Task<ActionResult<PersonaDto>> GetEmpleado(string idUsuario)
        {
            var cliente = await _basedatos.Clientes.Where(c => c.IdUsuario!.Contains(idUsuario)).FirstOrDefaultAsync();
            var persona = await _basedatos.Personas.FirstOrDefaultAsync(p => p.IdPersona == cliente.IdPersona);

            if (persona == null)
            {
                return NotFound($"No se encontró una persona con Id {cliente.IdPersona}");
            }

            var personaDto = MapPersonaDto(persona);

            return Ok(personaDto);
        }


        [HttpGet]
        [Authorize(Roles = "Empleado")]
        public async Task<ActionResult<IEnumerable<PersonaDto>>> GetEmpleados()
        {
            var personasDto = new List<PersonaDto>();
            var idsPersona = await _basedatos.Empleados.Select(e => e.IdPersona).ToListAsync();

            foreach (var id in idsPersona)
            {
                var persona = await _basedatos.Personas.FirstOrDefaultAsync(p => p.IdPersona == id);

                if (persona != null)
                {
                    var personaDto = MapPersonaDto(persona);
                    personasDto.Add(personaDto);
                }
                else
                {
                    Console.WriteLine($"No se encontró una persona con Id {id}");
                }
            }
            return Ok(personasDto);
        }

        private static PersonaDto MapPersonaDto(Persona persona) =>
           new PersonaDto
           {
               IdPersona = persona.IdPersona,
               Nombre = persona.Nombre,
               ApellidoPaterno = persona.ApellidoPaterno,
               ApellidoMaterno = persona.ApellidoMaterno,
               Direccion = persona.Direccion,
               Telefono = persona.Telefono,
               Email = persona.Email
           };

        [HttpPut]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> UpdatePersona(string idUsuario, PersonaDto personaDto)
        {
            var cliente = await _basedatos.Clientes.Where(c => c.IdUsuario!.Contains(idUsuario)).FirstOrDefaultAsync();
            var persona = await _basedatos.Personas.FirstOrDefaultAsync(p => p.IdPersona == cliente.IdPersona);

            if (persona == null)
            {
                return NotFound($"No se encontró una persona con Id {cliente.IdPersona}");
            }

            persona.Nombre = personaDto.Nombre;
            persona.ApellidoPaterno = personaDto.ApellidoPaterno;
            persona.ApellidoMaterno = personaDto.ApellidoMaterno;
            persona.Direccion = personaDto.Direccion;
            persona.Telefono = personaDto.Telefono;
            persona.Email = personaDto.Email;

            try
            {

                await _basedatos.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error actualizando la persona.");
            }

            return NoContent();
        }

    }

}
