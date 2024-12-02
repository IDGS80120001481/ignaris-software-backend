using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistorialVentasController : ControllerBase
    {
        private readonly LignarisPizzaContext _basedatos;

        public HistorialVentasController(LignarisPizzaContext basedatos)
        {
            _basedatos = basedatos;
        }

        [HttpGet]
        [Route("ventas")]
        public async Task<ActionResult<IEnumerable<HistorialVentaDto>>> GetTopVentas()
        {
            var ventas = await (from v in _basedatos.Venta
                                join c in _basedatos.Clientes on v.IdCliente equals c.IdCliente
                                join a in _basedatos.AppUser on c.IdUsuario equals a.Id
                                orderby v.IdVenta descending
                                select new HistorialVentaDto
                                {
                                    IdVenta = v.IdVenta,
                                    Total = v.Total,
                                    MetodoPago = v.MetodoPago,
                                    FechaVenta = v.FechaVenta,
                                    FullName = a.Fullname,
                                    HistorialVentaDetalleDto = new List<HistorialVentaDetalleDto>()
                                })
                                .Take(150)
                                .ToListAsync();

            if (ventas == null || ventas.Count == 0)
            {
                return NotFound("{\"result\": \"No se encontraron ventas.\"}");
            }

            foreach (var venta in ventas)
            {
                var detalles = await (from vd in _basedatos.VentaDetalles
                                      join r in _basedatos.Receta on vd.IdReceta equals r.IdReceta
                                      where vd.IdVenta == venta.IdVenta
                                      select new HistorialVentaDetalleDto
                                      {
                                          Nombre = r.Nombre,
                                          Cantidad = vd.Cantidad,
                                          PrecioUnitario = r.PrecioUnitario,
                                          Foto = r.Foto
                                      })
                                      .ToListAsync();

                if (detalles != null && detalles.Count > 0)
                {
                    venta.HistorialVentaDetalleDto.AddRange(detalles);
                }
            }
            return Ok(ventas);
        }


        [HttpGet]
        [Route("ventas/{idCliente}")]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentasPorCliente(int idCliente)
        {

            var cliente = await _basedatos.Clientes
                                    .Where(c => c.IdPersona == idCliente)
                                    .FirstOrDefaultAsync();

            // Realiza la consulta usando LINQ
            var ventas = await (from v in _basedatos.Venta
                                join c in _basedatos.Clientes
                                on v.IdCliente equals c.IdCliente
                                where v.IdCliente == cliente.IdCliente
                                orderby v.IdVenta descending
                                select new HistorialVentaDto
                                {
                                    IdVenta = v.IdVenta,
                                    Total = v.Total,
                                    MetodoPago = v.MetodoPago,
                                    FechaVenta = v.FechaVenta,
                                    HistorialVentaDetalleDto = new List<HistorialVentaDetalleDto>()
                                })
                               .ToListAsync();

            if (ventas == null || ventas.Count == 0)
            {
                return NotFound("{\"result\": \"No se encontraron ventas.\"}");
            }

            foreach (var venta in ventas)
            {
                var detalles = await (from vd in _basedatos.VentaDetalles
                                      join r in _basedatos.Receta on vd.IdReceta equals r.IdReceta
                                      where vd.IdVenta == venta.IdVenta
                                      select new HistorialVentaDetalleDto
                                      {
                                          Nombre = r.Nombre,
                                          Cantidad = vd.Cantidad,
                                          PrecioUnitario = r.PrecioUnitario,
                                          Foto = r.Foto
                                      })
                                      .ToListAsync();

                if (detalles != null && detalles.Count > 0)
                {
                    venta.HistorialVentaDetalleDto.AddRange(detalles);
                }
            }
            return Ok(ventas);

            return Ok(ventas);
        }
    }
}
