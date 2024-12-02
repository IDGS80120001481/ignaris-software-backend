using LignarisBack.Dto.crmdto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers.crmadds
{
    [Route("api/[controller]")]
    [ApiController]
    public class SugerenciasController : ControllerBase
    {
        private readonly LignarisPizzaContext _context;

        public SugerenciasController(LignarisPizzaContext context)
        {
            _context = context;
        }

        [HttpPut]
        [Route("updatehourcarttracking/{id}")]
        public async Task<IActionResult> UpdateHoraCarrito(int id, [FromBody] TimeSpan carttracking)
        {
            var horaNotificacion = await _context.HoraNotificacions.FindAsync(id);

            if (horaNotificacion == null)
            {
                return NotFound(new { mensaje = "Registro no encontrado" });
            }
            horaNotificacion.HoraSugerencias = carttracking;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error al actualizar", detalle = ex.Message });
            }

            return Ok(new { mensaje = "Hora del carrito actualizada exitosamente", horaActualizada = carttracking });
        }

        [HttpGet]
        [Route("recipelist")]
        public async Task<ActionResult<IEnumerable<RecetaSugerenciasDto>>> GetRecetas()
        {
            var recetas = await _context.Receta
                .Include(r => r.RecetaDetalles)
                .ThenInclude(rd => rd.IdMateriaPrimaNavigation)
                .ToListAsync();

            var RecetaDtos = recetas.Select(r => new RecetaSugerenciasDto
            {
                IdReceta = r.IdReceta,
                Nombre = r.Nombre,
                Foto = r.Foto,
                Tamanio = r.Tamanio,
                PrecioUnitario = r.PrecioUnitario,
                Estatus = r.Estatus,
                Suggest = r.suggest,
                RecetaDetalles = r.RecetaDetalles.Select(rd => new RecetaDetalleSugerenciasDto
                {
                    IdRecetaDetalle = rd.IdRecetaDetalle,
                    IdMateriaPrima = rd.IdMateriaPrima,
                    Cantidad = rd.Cantidad
                }).ToList()
            }).ToList();

            return RecetaDtos;
        }

        [HttpPut]
        [Route("addsuggest/{idReceta}")]
        public async Task<IActionResult> UpdateSuggest(int idReceta)
        {
            // Actualizar todas las pizzas a false
            await _context.Receta
                 .Where(r => r.suggest == true)
                 .ForEachAsync(r => r.suggest = false);
            await _context.SaveChangesAsync();

            // Agregar la pizza a las sugerencias
            var receta = await _context.Receta
                .FirstOrDefaultAsync(r => r.IdReceta == idReceta);

            if (receta == null)
            {
                return NotFound(new { message = "Receta no encontrada." });
            }

            receta.suggest = !receta.suggest;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Receta actualizada correctamente", success = true });
        }
    }
}
