using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LignarisBack.Dto.crmdto;
using LignarisBack.Dto;
using Microsoft.AspNetCore.Authorization;

namespace LignarisBack.Controllers.crmadds
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfertasController : ControllerBase
    {
        private readonly LignarisPizzaContext _context;

        public OfertasController(LignarisPizzaContext context)
        {
            _context = context;
        }

        // Insertar una oferta
        [HttpPost]
        [Route("addoffer")]
        public async Task<IActionResult> AddPersonalizedOffer([FromBody] PersonalizedOfferDto offerDto)
        {
            if (offerDto == null)
            {
                return BadRequest(new { message = "La oferta personalizada no puede estar vacía." });
            }

            if (offerDto.CantidadPizzas <= 0 || offerDto.CantidadDinero <= 0)
            {
                return BadRequest(new { message = "La cantidad de pizzas y el dinero deben ser mayores a cero." });
            }

            if (offerDto.InicioOferta >= offerDto.FinOferta)
            {
                return BadRequest(new { message = "La fecha de inicio debe ser anterior a la fecha de fin." });
            }

            // Verificar si el idRecetum existe en la base de datos
            var receta = await _context.Receta.FirstOrDefaultAsync(r => r.IdReceta == offerDto.idRecetum);
            if (receta == null)
            {
                return NotFound(new { message = $"No se encontró una receta con el ID {offerDto.idRecetum}." });
            }

            // Crear la entidad PersonalizedOffer
            var personalizedOffer = new PersonalizedOffer
            {
                idRecetum = offerDto.idRecetum,
                CantidadPizzas = offerDto.CantidadPizzas,
                CantidadDinero = offerDto.CantidadDinero,
                InicioOferta = offerDto.InicioOferta,
                FinOferta = offerDto.FinOferta,
            };

            // Agregar la nueva oferta a la base de datos
            try
            {
                _context.PersonalizedOffer.Add(personalizedOffer);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Oferta personalizada agregada correctamente", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al agregar la oferta personalizada.", detail = ex.Message });
            }
        }

        // Metodo para obtener las ofertas
        [HttpGet]
        [Route("getoffers")]
        public async Task<IActionResult> GetPersonalizedOffers()
        {
            try
            {
                var offers = await _context.PersonalizedOffer
                    .Select(o => new
                    {
                        o.idRecetum,
                        o.CantidadPizzas,
                        o.CantidadDinero,
                        o.InicioOferta,
                        o.FinOferta
                    })
                    .ToListAsync();

                if (offers == null || !offers.Any())
                {
                    return NotFound(new { message = "No se encontraron ofertas personalizadas." });
                }

                return Ok(new { message = "Ofertas personalizadas obtenidas correctamente.", data = offers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al obtener las ofertas personalizadas.", detail = ex.Message });
            }
        }

        // Metodo para eliminar las ofertas
        [HttpDelete]
        [Route("deleteoffer/{id}")]
        public async Task<IActionResult> DeletePersonalizedOffer(int id)
        {
            try
            {
                var offer = await _context.PersonalizedOffer.FindAsync(id);
                if (offer == null)
                {
                    return NotFound(new { message = $"No se encontró una oferta personalizada con el ID {id}." });
                }

                // Eliminar la oferta personalizada
                _context.PersonalizedOffer.Remove(offer);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Oferta personalizada eliminada correctamente", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ocurrió un error al eliminar la oferta personalizada.", detail = ex.Message });
            }
        }

        // Metodo para obtener el costo total de la orden de pizzas
        [HttpGet]
        [Route("getcosto")]
        public async Task<ActionResult<RecetaVentasDto>> GetCostoTotal(string idUsuario)
        {
            if (idUsuario == null)
            {
                return NotFound("{\"result\": \"No existe el registro de este carrito de compras.\"}");
            }

            // Se busca al usuario para agregar al carrito
            var cliente = await _context.Clientes
                                         .Where(c => c.IdUsuario!.Contains(idUsuario))
                                         .FirstOrDefaultAsync();

            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var compras = await _context.CarritoCompras
                                              .Where(c => c.IdCliente == cliente.IdCliente)
                                              .ToListAsync();

            Console.ForegroundColor = ConsoleColor.Blue;
            // Obtener las ofertas
            var ofertas = await _context.PersonalizedOffer
                    .Where(o => DateTime.Now >= o.InicioOferta && DateTime.Now <= o.FinOferta)
                    .ToListAsync();

            double descuento = 0;
            foreach (var o in ofertas)
            {
                var cantidad = await _context.CarritoCompras
                    .Where(c => c.IdRecetas == o.idRecetum)
                    .Select(c => c.Cantidad)
                    .FirstOrDefaultAsync();

                if (cantidad > 0)
                {
                    int cantidadpromo = (int)(cantidad / o.CantidadPizzas);
                    descuento += cantidadpromo * o.CantidadDinero;
                }
            }

            double? total = 0;

            foreach (var item in compras)
            {
               total += item.Cantidad * getPrecioReceta(item.IdRecetas);
            }
            total = total - descuento;

            return StatusCode(200, "{\"result\": \"" + total?.ToString("F2") + "\"}");
        }

        [HttpGet]
        [Route("getStatosOffer")]
        public async Task<IActionResult> GetCosto(string idUsuario)
        {
            if (idUsuario == null)
            {
                return NotFound("{\"result\": \"No existe el registro de este carrito de compras.\"}");
            }

            // Se busca al usuario para agregar al carrito
            var cliente = await _context.Clientes
                                         .Where(c => c.IdUsuario!.Contains(idUsuario))
                                         .FirstOrDefaultAsync();

            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            // Consultar usando LINQ
            var result = await (from o in _context.PersonalizedOffer
                                join c in _context.CarritoCompras on o.idRecetum equals c.IdRecetas
                                where c.IdCliente == cliente.IdCliente
                                group new { o, c } by new { o.CantidadPizzas, c.Cantidad, c.IdCliente } into grouped
                                select new
                                {
                                    grouped.Key.CantidadPizzas,
                                    grouped.Key.Cantidad,
                                    porcentaje = (100.0 / grouped.Key.CantidadPizzas) * grouped.Key.Cantidad
                                }).ToListAsync();

            // Retornar el resultado
            return Ok(result);
        }


        // Obtener el precio por cada receta
        private double? getPrecioReceta(int idReceta)
        {
            var receta = _context.Receta.Find(idReceta);
            if (receta == null)
            {
                throw new KeyNotFoundException($"Receta con ID {idReceta} no encontrada.");
            }
            return receta.PrecioUnitario;
        }

    }
}
