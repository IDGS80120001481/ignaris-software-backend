using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LignarisBack.Controllers.crmadds
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeguimientoController : ControllerBase
    {
        private readonly LignarisPizzaContext _context;

        public SeguimientoController(LignarisPizzaContext context)
        {
            _context = context;
        }

        [HttpPut]
        [Route("updatehourcart/{id}")]
        public async Task<IActionResult> UpdateHoraCarrito(int id, [FromBody] TimeSpan nuevaHoraCarrito)
        {
            var horaNotificacion = await _context.HoraNotificacions.FindAsync(id);

            if (horaNotificacion == null)
            {
                return NotFound(new { mensaje = "Registro no encontrado" });
            }
            horaNotificacion.HoraCarrito = nuevaHoraCarrito;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error al actualizar", detalle = ex.Message });
            }

            return Ok(new { mensaje = "Hora del carrito actualizada exitosamente", horaActualizada = nuevaHoraCarrito });
        }


    }
}
