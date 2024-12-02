using LignarisBack.Models;
using LignarisBack.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Lignaris_Pizza_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : Controller
    {
        private readonly LignarisPizzaContext _baseDatos;

        public InventarioController(LignarisPizzaContext baseDatos)
        {
            _baseDatos = baseDatos;
        }

        // Método GET ListaInventario que devuelve la lista de todas las tareas en la BD
        /*[HttpGet]
        [Route("ListaInventario")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> Lista()
        {
            var listaMateriasPrimas = await _baseDatos.Inventarios
                .Join(_baseDatos.CompraDetalles,
                      inventario => inventario.IdCompraDetalle,
                      compraDetalle => compraDetalle.IdCompraDetalle,
                      (inventario, compraDetalle) => new { inventario, compraDetalle })
                .Join(_baseDatos.Compras,
                      inventarioYdetalle => inventarioYdetalle.compraDetalle.IdCompra,
                      compra => compra.IdCompra,
                      (inventarioYdetalle, compra) => new { inventarioYdetalle.inventario, inventarioYdetalle.compraDetalle, compra })
                .Join(_baseDatos.MateriaPrimas,
                      inventarioYdetalleYcompra => inventarioYdetalleYcompra.compraDetalle.IdMateriaPrima,
                      materiaPrima => materiaPrima.IdMateriaPrima,
                      (inventarioYdetalleYcompra, materiaPrima) => new InventarioDetalleDto
                      {
                          IdInventario = inventarioYdetalleYcompra.inventario.IdInventario,
                          Nombre = materiaPrima.Nombre,
                          CantidadDisponible = inventarioYdetalleYcompra.inventario.CantidadDisponible,
                          FechaCompra = inventarioYdetalleYcompra.compra.FechaCompra,
                          FechaCaducidad = inventarioYdetalleYcompra.compraDetalle.FechaCaducidad,
                          NumLote = inventarioYdetalleYcompra.compraDetalle.NumLote,
                          Estatus = inventarioYdetalleYcompra.inventario.Estatus
                      })
                .ToListAsync();

            return Ok(listaMateriasPrimas);
        } */

        [HttpGet]
        [Route("ListaInventario")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> Lista()
        {
            var listaMateriasPrimas = await _baseDatos.Inventarios
                .Join(_baseDatos.CompraDetalles,
                    inventario => inventario.IdCompraDetalle,
                    compraDetalle => compraDetalle.IdCompraDetalle,
                    (inventario, compraDetalle) => new { inventario, compraDetalle })
                .Join(_baseDatos.Compras,
                    inventarioYdetalle => inventarioYdetalle.compraDetalle.IdCompra,
                    compra => compra.IdCompra,
                    (inventarioYdetalle, compra) => new { inventarioYdetalle.inventario, inventarioYdetalle.compraDetalle, compra })
                .Join(_baseDatos.MateriaPrimas,
                    inventarioYdetalleYcompra => inventarioYdetalleYcompra.compraDetalle.IdMateriaPrima,
                    materiaPrima => materiaPrima.IdMateriaPrima,
                    (inventarioYdetalleYcompra, materiaPrima) => new
                    {
                        materiaPrima.Nombre,
                        inventarioYdetalleYcompra.inventario.CantidadDisponible
                    })
                .GroupBy(x => new { x.Nombre })
                .Select(g => new InventarioProporcionesDto
                {
                    Nombre = g.Key.Nombre,
                    Miligramos = g.Sum(x => x.CantidadDisponible),
                    Kilogramos = g.Sum(x => x.CantidadDisponible) / 1000,
                    Toneladas = g.Sum(x => x.CantidadDisponible) / 1000000
                })
                .OrderBy(x => x.Nombre)  // O también por cualquier otro campo que prefieras
                .ToListAsync();

            return Ok(listaMateriasPrimas);
        }

    }
}
