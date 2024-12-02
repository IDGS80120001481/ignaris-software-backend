using LignarisBack.Models;
using LignarisBack.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {

        private readonly LignarisPizzaContext _basedatos;

        public VentaController(LignarisPizzaContext basedatos)
        {
            _basedatos = basedatos;
        }

        [HttpPost]
        public async Task<ActionResult<VentaDto>> InsertVenta(string idUsuario, double total, int estatus, string metodopago)
        {
            if (idUsuario == null)
            {
                return NotFound("{\"result\": \"No existe el registro de este carrito de compras.\"}");
            }

            // Se busca al usuario para agregar al carrito
            var cliente = await _basedatos.Clientes
                                         .Where(c => c.IdUsuario!.Contains(idUsuario))
                                         .FirstOrDefaultAsync();

            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            DateOnly currenttime = DateOnly.FromDateTime(DateTime.Now);
            var venta = new Ventum
            {
                IdEmpleado = 8,
                IdCliente = cliente.IdCliente,
                Estatus = estatus,
                FechaVenta = currenttime,
                Total = total,
                MetodoPago = metodopago 

            };

            _basedatos.Venta.Add(venta);
            await _basedatos.SaveChangesAsync();
            int idVenta = venta.IdVenta;

            var carrito = await _basedatos.CarritoCompras
                .Where(c => c.IdCliente == cliente.IdCliente)
                .ToListAsync();

            // Inventario
            var inventario = from cd in _basedatos.CompraDetalles
                             join i in _basedatos.Inventarios
                             on cd.IdCompraDetalle equals i.IdCompraDetalle
                             group i by cd.IdMateriaPrima into grouped
                             select new
                             {
                                 IdMateriaPrima = grouped.Key,
                                 TotalCantidadDisponible = grouped.Sum(x => x.CantidadDisponible)
                             };
            var inventarioList = inventario.ToList();

            if (!carrito.Any())
            {
                return BadRequest("No existe el producto en el carrito de compras.");
            }

            foreach (var c in carrito)
            {
                var ingredientes = await _basedatos.RecetaDetalles
                    .Where(r => r.IdReceta == c.IdRecetas).ToListAsync();

                foreach (var ing in ingredientes)
                {
                    foreach (var rec in inventarioList)
                    {
                        if (ing.IdMateriaPrima == rec.IdMateriaPrima)
                        {
                            if (rec.TotalCantidadDisponible < ing.Cantidad)
                            {
                                return BadRequest(new { result = "No hay material suficiente para la venta." });
                            }
                        }

                    }
                }

                var detalleVenta = new VentaDetalle
                {
                    IdVenta = idVenta,
                    IdReceta = c.IdRecetas,
                    Cantidad = c.Cantidad
                };

                _basedatos.VentaDetalles.Add(detalleVenta);
                _basedatos.CarritoCompras.Remove(c);

            }

            await _basedatos.SaveChangesAsync();
            return Ok(new { result = "Se ha realizado correctamente la venta." });
        }

        [HttpPut]
        [Route("estatus")]
        public async Task<IActionResult> DisminuirCarritoCompras(int IdVenta, int estatus)
        {
            // Validación de ID de venta
            if (IdVenta == 0)
            {
                return BadRequest("{\"result\": \"El ID de la venta no puede estar vacío.\"}");
            }

            try
            {
                // Buscar la venta
                var venta = await _basedatos.Venta
                                             .FirstOrDefaultAsync(c => c.IdVenta == IdVenta);

                if (venta == null)
                {
                    return NotFound("{\"result\": \"Venta no encontrada.\"}");
                }

                // Obtener los detalles de la venta
                var detalles = await _basedatos.VentaDetalles
                    .Where(d => d.IdVenta == venta.IdVenta).ToArrayAsync();

                // Obtener inventario agrupado por materia prima
                var inventario = await (from cd in _basedatos.CompraDetalles
                                        join i in _basedatos.Inventarios
                                        on cd.IdCompraDetalle equals i.IdCompraDetalle
                                        group i by cd.IdMateriaPrima into grouped
                                        select new
                                        {
                                            IdMateriaPrima = grouped.Key,
                                            TotalCantidadDisponible = grouped.Sum(x => x.CantidadDisponible)
                                        }).ToListAsync();

                // Procesar la disminución del inventario
                foreach (var detalle in detalles)
                {
                    var detalleReceta = await _basedatos.RecetaDetalles
                            .Where(d => d.IdReceta == detalle.IdReceta).ToArrayAsync();

                    foreach (var dr in detalleReceta)
                    {
                        var inv = inventario.FirstOrDefault(i => i.IdMateriaPrima == dr.IdMateriaPrima);

                        if (inv != null)
                        {
                            var detalleCompra = await _basedatos.CompraDetalles
                                .FirstOrDefaultAsync(d => d.IdMateriaPrima == inv.IdMateriaPrima);

                            if (detalleCompra != null)
                            {
                                var inventariodb = await _basedatos.Inventarios
                                    .FirstOrDefaultAsync(i => i.IdCompraDetalle == detalleCompra.IdCompraDetalle);

                                if (inventariodb != null)
                                {
                                    inventariodb.CantidadDisponible -= dr.Cantidad;
                                }
                            }
                        }
                    }
                }

                // Actualizar estatus de la venta
                venta.Estatus = estatus;
                await _basedatos.SaveChangesAsync();

                return Ok("{\"result\": \"El estatus se ha cambiado correctamente.\"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Error interno del servidor.");
            }
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaProduccionDto>>> GetVentasDisponibles()
        {

            var venta = await _basedatos.Venta.Where(v => v.Estatus == 1 || v.Estatus == 2).ToListAsync();

            if (venta == null)
            {
                return NotFound("No se encontraron ventas con el estatus indicado");
            }

            List<EntregaProductosDto> ventaList = new List<EntregaProductosDto>();
            foreach (var v in venta)
            {
                EntregaProductosDto ventaDto = new EntregaProductosDto();

                ventaDto.IdVenta = v.IdVenta;
                ventaDto.FechaVenta = v.FechaVenta;
                ventaDto.Total = v.Total;
                if (v.Estatus == 1)
                {
                    ventaDto.Pagado = "Pagado";
                }
                else
                {
                    ventaDto.Pagado = "No Pagado";
                }

                List<VentaDetalleProduccionDto>? detallesList = new List<VentaDetalleProduccionDto>();

                var bd_detalles = await _basedatos.VentaDetalles.Where(d => d.IdVenta == ventaDto.IdVenta).ToListAsync();

                foreach (var d in bd_detalles)
                {
                    VentaDetalleProduccionDto detalles = new VentaDetalleProduccionDto();
                    var receta = await _basedatos.Receta.FirstAsync(r => r.IdReceta == d.IdReceta);

                    if (receta == null)
                    {
                        return NotFound("No se pudieron obtener las recetas.");
                    }

                    detalles.Nombre = receta.Nombre;
                    detalles.Cantidad = d.Cantidad;
                    detalles.Foto = receta.Foto;
                    string tamanio = "";
                    switch (receta.Tamanio)
                    {
                        case 1:
                            tamanio = "Chica";
                            break;
                        case 2:
                            tamanio = "Mediana";
                            break;
                        case 3:
                            tamanio = "Grande";
                            break;
                        case 4:
                            tamanio = "Jumbo";
                            break;
                    }

                    detalles.Tamanio = tamanio;
                    detallesList.Add(detalles);
                }
                ventaDto.DetalleVenta = detallesList;
                ventaList.Add(ventaDto);
            }



            return Ok(ventaList);
        }


        // Metodo para visualizar entrega de productos
        [HttpGet]
        [Route("entrega")]
        public async Task<ActionResult<IEnumerable<EntregaProductosDto>>> GetEntregaizzas()
        {

            var venta = await _basedatos.Venta.Where(v => v.Estatus == 3 || v.Estatus == 4).ToListAsync();
            if (venta == null)
            {
                return NotFound("No se encontraron ventas con el estatus indicado");
            }

            List<EntregaProductosDto> ventaList = new List<EntregaProductosDto>();
            foreach (var v in venta)
            {
                EntregaProductosDto ventaDto = new EntregaProductosDto();

                ventaDto.IdVenta = v.IdVenta;
                ventaDto.FechaVenta = v.FechaVenta;
                ventaDto.Total = v.Total;
                if (v.Estatus == 3)
                {
                    ventaDto.Pagado = "Pagado";
                }
                else
                {
                    ventaDto.Pagado = "No Pagado";
                }

                var cliente = await _basedatos.Clientes.Where(c => c.IdCliente == v.IdCliente).FirstAsync();
                var persona = await _basedatos.Personas.Where(p => p.IdPersona == cliente.IdPersona).FirstAsync();
                Console.WriteLine(persona.Nombre);

                ventaDto.Persona = new PersonaDto(); 

                ventaDto.Persona.Nombre = persona.Nombre;
                ventaDto.Persona.ApellidoMaterno = persona.ApellidoPaterno;
                ventaDto.Persona.ApellidoPaterno = persona.ApellidoMaterno;
                ventaDto.Persona.Telefono = persona.Telefono;
                ventaDto.Persona.Direccion = persona.Direccion;
                ventaDto.Persona.Email = persona.Email;


                List<VentaDetalleProduccionDto>? detallesList = new List<VentaDetalleProduccionDto>();

                var bd_detalles = await _basedatos.VentaDetalles.Where(d => d.IdVenta == ventaDto.IdVenta).ToListAsync();

                foreach (var d in bd_detalles)
                {
                    VentaDetalleProduccionDto detalles = new VentaDetalleProduccionDto();
                    var receta = await _basedatos.Receta.FirstAsync(r => r.IdReceta == d.IdReceta);

                    if (receta == null)
                    {
                        return NotFound("No se pudieron obtener las recetas.");
                    }

                    detalles.Nombre = receta.Nombre;
                    detalles.Cantidad = d.Cantidad;
                    detalles.Foto = receta.Foto;
                    detalles.PrecioUnitario = receta.PrecioUnitario;
                    string tamanio = "";
                    switch (receta.Tamanio)
                    {
                        case 1:
                            tamanio = "Chica";
                            break;
                        case 2:
                            tamanio = "Mediana";
                            break;
                        case 3:
                            tamanio = "Grande";
                            break;
                        case 4:
                            tamanio = "Jumbo";
                            break;
                    }

                    detalles.Tamanio = tamanio;
                    detallesList.Add(detalles);
                }
                ventaDto.DetalleVenta = detallesList;
                ventaList.Add(ventaDto);
            }
            return Ok(ventaList);
        }
    }
}
