using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphicsController : ControllerBase
    {

        private readonly LignarisPizzaContext _basedatos;

        public GraphicsController(LignarisPizzaContext basedatos)
        {
            _basedatos = basedatos;
        }

        // Gráfica para contar el número de comentarios por cliente
        [HttpPost]
        [Route("getComentarioCount")]
        public async Task<ActionResult> GetComentarioCount()
        {
            var resultado = await _basedatos.Comentarios
                .Join(
                    _basedatos.Clientes,
                    comentario => comentario.IdCliente,
                    cliente => cliente.IdCliente,
                    (comentario, cliente) => new { comentario, cliente }
                )
                .Join(
                    _basedatos.AppUser,
                    combined => combined.cliente.IdUsuario,
                    usuario => usuario.Id,
                    (combined, usuario) => new { combined.comentario, combined.cliente, usuario }
                )
                .GroupBy(x => new { x.cliente.IdCliente, x.usuario.Fullname })
                .Select(g => new
                {
                    ClienteId = g.Key.IdCliente,
                    NombreCompleto = g.Key.Fullname,
                    ConteoComentarios = g.Count()
                })
                .ToListAsync();

            if (resultado == null || !resultado.Any())
            {
                return NotFound("{\"result\": \"No se encontraron comentarios.\"}");
            }

            return Ok(resultado);
        }

        // Grafica para contar los comentarios según el tipo de comentario
        [HttpPost]
        [Route("getComentarioTipoCount")]
        public async Task<ActionResult> GetComentarioTipoCount()
        {
            var resultado = await _basedatos.Comentarios
                .GroupBy(c => c.TipoComentario)
                .Select(g => new
                {
                    TipoComentario = g.Key,
                    CantidadComentarios = g.Count()
                })
                .ToListAsync();

            if (resultado == null || !resultado.Any())
            {
                return NotFound("{\"result\": \"No se encontraron comentarios.\"}");
            }
            return Ok(resultado);
        }

        // Graficas para contar los comentarios según el tipo de receta
        [HttpPost]
        [Route("getComentarioPorReceta")]
        public async Task<ActionResult> GetComentarioPorReceta()
        {
            var resultado = await _basedatos.Comentarios
                .Join(
                    _basedatos.Receta,
                    comentario => comentario.IdReceta,
                    receta => receta.IdReceta,
                    (comentario, receta) => new { comentario, receta }
                )
                .GroupBy(x => x.receta.Nombre)
                .Select(g => new
                {
                    NombreReceta = g.Key,
                    CantidadComentarios = g.Count()
                })
                .ToListAsync();

            if (resultado == null || !resultado.Any())
            {
                return NotFound("{\"result\": \"No se encontraron comentarios para las recetas.\"}");
            }
            return Ok(resultado);
        }

        // Grafica ventas por cliente que mas ha comprado
        [HttpPost]
        [Route("getVentasPorCliente")]
        public async Task<ActionResult> GetVentasPorCliente()
        {
            var resultado = await _basedatos.Venta
                .Join(
                    _basedatos.Clientes,
                    venta => venta.IdCliente,
                    cliente => cliente.IdCliente,
                    (venta, cliente) => new { venta, cliente }
                )
                .Join(
                    _basedatos.AppUser,
                    combined => combined.cliente.IdUsuario,
                    usuario => usuario.Id,
                    (combined, usuario) => new { combined.venta, combined.cliente, usuario }
                )
                .GroupBy(x => x.usuario.Fullname)
                .Select(g => new
                {
                    NombreCompleto = g.Key,
                    TotalVentas = g.Sum(x => x.venta.Total)
                })
                .ToListAsync();

            if (resultado == null || !resultado.Any())
            {
                return NotFound("{\"result\": \"No se encontraron ventas.\"}");
            }
            return Ok(resultado);
        }

        // Pizzas Preferidas por los clientes
        [HttpPost]
        [Route("getCantidadPizzaPorReceta")]
        public async Task<ActionResult> GetCantidadPizzaPorReceta()
        {
            var resultado = await _basedatos.VentaDetalles
                .Join(
                    _basedatos.Receta,
                    ventaDetalle => ventaDetalle.IdReceta,
                    receta => receta.IdReceta,
                    (ventaDetalle, receta) => new { ventaDetalle, receta }
                )
                .GroupBy(x => x.receta.Nombre)
                .Select(g => new
                {
                    NombreReceta = g.Key,
                    CantidadPizza = g.Sum(x => x.ventaDetalle.Cantidad)
                })
                .ToListAsync();

            if (resultado == null || !resultado.Any())
            {
                return NotFound("{\"result\": \"No se encontraron ventas de pizzas.\"}");
            }
            return Ok(resultado);
        }

        // Cantidad de pizzas por orden adquiridas por los clientes
        [HttpPost]
        [Route("getConteoPorCantidad")]
        public async Task<ActionResult> GetConteoPorCantidad()
        {
            var resultado = await _basedatos.VentaDetalles
                .GroupBy(vd => vd.Cantidad)
                .Select(g => new
                {
                    Cantidad = g.Key,
                    Conteo = g.Count()
                })
                .ToListAsync();

            if (resultado == null || !resultado.Any())
            {
                return NotFound("{\"result\": \"No se encontraron registros en venta_detalle.\"}");
            }
            return Ok(resultado);
        }
    }
}
