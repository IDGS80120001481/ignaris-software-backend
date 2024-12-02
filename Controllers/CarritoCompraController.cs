using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarritoCompraController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly LignarisPizzaContext _basedatos;

        public CarritoCompraController(UserManager<AppUser> userManager, RoleManager<IdentityRole>
       roleManager, IConfiguration configuration, LignarisPizzaContext basedatos)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _basedatos = basedatos;
        }

        /*
         * Metodo para obtener las recetas para el carrito de compra para ser mostradas en la interfaz del
         * carrito de compras
         */
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecetaVentasDto>>> GetRecetasVenta()
        {

            return await _basedatos.Receta
                .Select(x => MapRecetaDto(x))
                .ToListAsync();
        }

        /*
         * Metodo para obtener las recetas para el carrito de compra para ser mostradas en la interfaz del
         * carrito de compras
         */
        [HttpGet]
        [Route("CarritoCliente")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<IEnumerable<CarritoClienteDto>>> GetCarritoCliente(string idUsuario)
        {
            if (string.IsNullOrEmpty(idUsuario))
            {
                return NotFound("{\"result\": \"No existe el registro de este carrito de compras.\"}");
            }

            var cliente = await _basedatos.Clientes
                                         .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var resultado = _basedatos.CarritoCompras
                          .Join(
                              _basedatos.Receta,
                              carrito => carrito.IdRecetas,
                              receta => receta.IdReceta,
                              (carrito, receta) => new
                              {
                                  Carrito = carrito,
                                  Receta = receta
                              }
                          )
                          .ToList();

            List<CarritoClienteDto> carrito = new List<CarritoClienteDto>();

            foreach (var item in resultado)
            {
                var value = new CarritoClienteDto
                {
                    IdReceta = item.Receta.IdReceta,
                    Nombre = item.Receta.Nombre,
                    Foto = item.Receta.Foto,
                    Tamanio = item.Receta.Tamanio,
                    PrecioUnitario = item.Receta.PrecioUnitario,
                    Cantidad = item.Carrito.Cantidad,
                };

                carrito.Add(value);
            }

            return carrito;
        }


        /*
         * Metodo para obtener un solo elemento del carrito de compras
         */
        [HttpGet("{id}")]
        public async Task<ActionResult<RecetaVentasDto>> GetRecetaIDVenta(int id)
        {
            var receta = await _basedatos.Receta.FindAsync(id);

            if (receta == null)
            {
                return NotFound();
            }

            return MapRecetaDto(receta);
        }

        [HttpGet]
        [Route("costo")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<RecetaVentasDto>> GetCostoTotal(string idUsuario)
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

            var compras = await _basedatos.CarritoCompras
                                              .Where(c => c.IdCliente == cliente.IdCliente)
                                              .ToListAsync();

            double? total = 0;
            foreach (var item in compras)
            {
                total += item.Cantidad * getPrecioReceta(item.IdRecetas);
            }

            return StatusCode(200, "{\"result\": \"" + total + "\"}");
        }

        private double? getPrecioReceta(int idReceta)
        {
            var receta = _basedatos.Receta.Find(idReceta);

            return receta!.PrecioUnitario;
        }

        private static RecetaVentasDto MapRecetaDto(Recetum receta) =>
       new RecetaVentasDto
       {
           IdReceta = receta.IdReceta,
           Nombre = receta.Nombre,
           Descripcion = receta.descripcion,
           Foto = receta.Foto,
           Tamanio = receta.Tamanio,
           PrecioUnitario = receta.PrecioUnitario,
           Estatus = receta.Estatus
       };


        /*
         * Metodo para agregar elementos a mi carrito de compras
         */
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> AgregarCarritoCompra(string idUsuario, int IdReceta)
        {
            if (string.IsNullOrEmpty(idUsuario))
            {
                return BadRequest("{\"result\": \"El ID de usuario no puede estar vacío.\"}");
            }

            try
            {
                // Se busca al usuario para agregar al carrito
                var cliente = await _basedatos.Clientes
                                             .Where(c => c.IdUsuario!.Contains(idUsuario))
                                             .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return NotFound("{\"result\": \"Cliente no encontrado.\"}");
                }

                int Cantidad = 1;
                var compras = await _basedatos.CarritoCompras
                                              .Where(c => c.IdCliente == cliente.IdCliente)
                                              .Where(c => c.IdRecetas == IdReceta)
                                              .FirstOrDefaultAsync();

                if (compras == null)
                {
                    var carrito = new CarritoCompras
                    {
                        IdRecetas = IdReceta,
                        IdCliente = cliente.IdCliente,
                        Cantidad = Cantidad
                    };

                    await _basedatos.CarritoCompras.AddAsync(carrito);
                    await _basedatos.SaveChangesAsync();
                    return StatusCode(200, "{\"result\": \"Se ha insertado un nuevo objeto al carrito de compras.\"}");
                }
                else
                {
                    compras!.Cantidad += 1;
                    await _basedatos.SaveChangesAsync();
                    return StatusCode(200, "{\"result\": \"Se ha agregado un objeto item en el carrito de compras.\"}");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "{\"result\": \"Error interno del servidor.\"}");
            }
        }


        [HttpPut]
        [Route("aumentar")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> AumentarCarritoCompras(string idUsuario, int IdReceta)
        {
            if (string.IsNullOrEmpty(idUsuario))
            {
                return BadRequest("{\"result\": \"El ID de usuario no puede estar vacío.\"}");
            }

            try
            {
                // Se busca al usuario para agregar al carrito
                var cliente = await _basedatos.Clientes
                                             .Where(c => c.IdUsuario!.Contains(idUsuario))
                                             .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return BadRequest("{\"result\": \"Cliente no encontrado.\"}");
                }

                // Buscar el elemento en el carrito de compras
                var compras = await _basedatos.CarritoCompras
                                              .Where(c => c.IdCliente == cliente.IdCliente)
                                              .Where(c => c.IdRecetas == IdReceta)
                                              .FirstOrDefaultAsync();

                if (compras != null)
                {
                    // Aumentar el elemento en el carrito de compras
                    compras!.Cantidad += 1;
                    await _basedatos.SaveChangesAsync();
                    return StatusCode(200, "{\"result\": \"Se ha aumentado correctamente el articulo.\"}");
                }
                return NotFound("{\"result\": \"Elemento no encontrado en el carrito de compras.\"}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpPut]
        [Route("disminuir")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> DisminuirCarritoCompras(string idUsuario, int IdReceta)
        {
            // Valida si el id de usuario no esta vacio
            if (string.IsNullOrEmpty(idUsuario))
            {
                return BadRequest("{\"result\": \"El ID de usuario no puede estar vacío.\"}");
            }

            try
            {
                // Se busca al usuario para agregar al carrito
                var cliente = await _basedatos.Clientes
                                             .Where(c => c.IdUsuario!.Contains(idUsuario))
                                             .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return NotFound("{\"result\": \"Cliente no encontrado.\"}");
                }

                // Buscar el elemento en el carrito de compras
                var compras = await _basedatos.CarritoCompras
                                              .Where(c => c.IdCliente == cliente.IdCliente)
                                              .Where(c => c.IdRecetas == IdReceta)
                                              .FirstOrDefaultAsync();

                if (compras != null)
                {
                    int value = compras!.Cantidad - 1;
                    if (value > 0)
                    {
                        // Disminuir el elemento en el carrito de compras
                        compras!.Cantidad -= 1;
                        await _basedatos.SaveChangesAsync();
                        return StatusCode(200, "Se ha disminuido correctamente el articulo.");
                    }
                    else
                    {
                        _basedatos.CarritoCompras.Remove(compras!);
                        await _basedatos.SaveChangesAsync();
                        return StatusCode(200, "Se ha eliminado correctamente el articulo.");
                    }
                }
                return NotFound("Elemento no encontrado en el carrito de compras.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Error interno del servidor.");
            }
        }

        [HttpDelete]
        [Route("delete/{idCarrito:int}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> EliminarProducto(int idCarrito)
        {
            var compras = await _basedatos.CarritoCompras.Where(c => c.IdRecetas == idCarrito).FirstAsync();

            if (compras == null)
            {
                return BadRequest("No existe el producto en el carrito de compras.");
            }

            _basedatos.CarritoCompras.Remove(compras);
            await _basedatos.SaveChangesAsync();

            return StatusCode(200, "Se ha eliminado correctamente el producto del carrito de compras.");
        }


        // Funcion que nos ayuda a obtener las compras realizadas por el cliente
        [HttpGet]
        [Route("miscompras")]
        public async Task<ActionResult<IEnumerable<EntregaProductosDto>>> GetMisCompras(string idUsuario)
        {


            var cliente = await _basedatos.Clientes
                                         .Where(c => c.IdUsuario!.Contains(idUsuario))
                                         .FirstOrDefaultAsync();
            var venta = await _basedatos.Venta
                .Where(v => v.Estatus == 0 && v.IdCliente == cliente.IdCliente)
                .OrderByDescending(v => v.IdVenta)
                .ToListAsync();

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
                else {
                    ventaDto.Pagado = "No Pagado";
                }

                

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

                double Total = 0;
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
                    Total += (double)receta.PrecioUnitario * (int)d.Cantidad;
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
                ventaDto.Total = Total;
                ventaDto.DetalleVenta = detallesList;
                ventaList.Add(ventaDto);
            }
            return Ok(ventaList);
        }
    }
}
