using LignarisBack.Models;
using LignarisBack.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lignaris_Pizza_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly LignarisPizzaContext _baseDatos;

        public DashboardController(LignarisPizzaContext baseDatos)
        {
            _baseDatos = baseDatos;
        }

        [Route("ventas-empleados")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaEmpleadoDTO>>> GetVentasEmpleado()
        {
            var result = await (from v in _baseDatos.Venta
                                join vd in _baseDatos.VentaDetalles on v.IdVenta equals vd.IdVenta
                                join e in _baseDatos.Empleados on v.IdEmpleado equals e.IdEmpleado
                                join p in _baseDatos.Personas on e.IdPersona equals p.IdPersona
                                group new { v, vd } by new
                                {
                                    p.Nombre,
                                    p.ApellidoPaterno,
                                    p.ApellidoMaterno,
                                    v.IdVenta,
                                    v.FechaVenta,
                                    v.Total
                                } into g
                                select new VentaEmpleadoDTO
                                {
                                    NombreEmpleado = g.Key.Nombre + " " + g.Key.ApellidoPaterno + " " + g.Key.ApellidoMaterno,
                                    IdVenta = g.Key.IdVenta,
                                    Cantidad = g.Sum(x => x.vd.Cantidad) ?? 0,
                                    FechaVenta = g.Key.FechaVenta ?? default(DateOnly),
                                    Total = g.Key.Total ?? 0.0
                                })
                                .ToListAsync();

            return Ok(result);
        }

        [Route("detalles-producto-mas-vendido/{id_receta:int}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoMasVendidoDTO>>> GetDetallesProductoMasVendido(int id_receta)
        {
            var result = await (from v in _baseDatos.Venta
                                join vd in _baseDatos.VentaDetalles on v.IdVenta equals vd.IdVenta
                                join r in _baseDatos.Receta on vd.IdReceta equals r.IdReceta
                                where vd.IdReceta == id_receta
                                group new { v, vd, r } by new
                                {
                                    vd.IdReceta,
                                    v.FechaVenta,
                                    r.Nombre
                                } into g
                                select new DetallesProductoMasVendidoDTO
                                {
                                    NombreReceta = g.Key.Nombre,
                                    IdReceta = g.Key.IdReceta,
                                    TotalCantidad = g.Sum(x => x.vd.Cantidad) ?? 0,
                                    FechaVenta = g.Key.FechaVenta ?? default(DateOnly),
                                    TotalVentas = g.Sum(x => x.v.Total) ?? 0.0
                                })
                                .OrderByDescending(x => x.TotalVentas)
                                .ToListAsync();

            return Ok(result);
        }

        [Route("producto-mas-vendido")]
        [HttpGet]
        public async Task<ActionResult<ProductoMasVendidoDTO>> GetProductoMasVendido()
        {
            var result = await (from vd in _baseDatos.VentaDetalles
                                join v in _baseDatos.Venta on vd.IdVenta equals v.IdVenta
                                join r in _baseDatos.Receta on vd.IdReceta equals r.IdReceta
                                group vd by new { vd.IdReceta, r.Nombre, r.PrecioUnitario } into g
                                orderby g.Sum(x => x.Cantidad) descending
                                select new ProductoMasVendidoDTO
                                {
                                    IdReceta = g.Key.IdReceta,
                                    NombreReceta = g.Key.Nombre,
                                    PizzasVendidas = g.Sum(x => x.Cantidad ?? 0),
                                    TotalVendido = g.Sum(x => (double)(x.Cantidad ?? 0) * (g.Key.PrecioUnitario ?? 0.0)) // Conversión explícita
                                })
                                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound("No se encontró ningún producto más vendido.");
            }

            return Ok(result);
        }

        [Route("ventas-productos")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoVentasDTO>>> GetVentasProductos()
        {
            var result = await (from v in _baseDatos.Venta
                                join vd in _baseDatos.VentaDetalles on v.IdVenta equals vd.IdVenta
                                join r in _baseDatos.Receta on vd.IdReceta equals r.IdReceta
                                group new { vd, v, r } by new { vd.IdReceta, v.FechaVenta, r.Nombre } into g
                                orderby g.Sum(x => x.v.Total) descending
                                select new ProductoVentasDTO
                                {
                                    IdReceta = g.Key.IdReceta,
                                    NombreReceta = g.Key.Nombre,
                                    TotalCantidad = g.Sum(x => x.vd.Cantidad ?? 0),
                                    FechaVenta = g.Key.FechaVenta ?? default(DateOnly),
                                    TotalVentas = g.Sum(x => x.v.Total ?? 0.0)
                                })
                                .ToListAsync();

            if (!result.Any())
            {
                return NotFound("No se encontraron ventas de productos.");
            }

            return Ok(result);
        }

    }
}
