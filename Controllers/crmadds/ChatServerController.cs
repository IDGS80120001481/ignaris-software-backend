using FirebaseAdmin.Messaging;
using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers.crmadds
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatServerController : ControllerBase
    {
        private readonly LignarisPizzaContext _basedatos;

        public ChatServerController(LignarisPizzaContext basedatos)
        {
            _basedatos = basedatos;
        }

        [HttpPost("InsertarMensaje")]
        public async Task<ActionResult> InsertarMensaje([FromBody] MessageDto mensajeDto)
        {
            if (mensajeDto == null)
            {
                return BadRequest("{\"result\": \"Datos del mensaje no proporcionados.\"}");
            }

            var cliente = await _basedatos.Clientes
                                        .FirstOrDefaultAsync(c => c.IdPersona == mensajeDto.IdCliente);
            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var empleado = await _basedatos.Empleados
                                          .FirstOrDefaultAsync(e => e.IdUsuario == mensajeDto.IdEmpleado);
            if (empleado == null)
            {
                return NotFound("{\"result\": \"Empleado no encontrado.\"}");
            }

            var mensaje = new ChatServer
            {
                IdCliente = mensajeDto.IdCliente,
                IdEmpleado = empleado.IdEmpleado,
                Message = mensajeDto.Message,
                View = mensajeDto.View,
                Send = mensajeDto.Send,
                Cliente = cliente,
                Empleado = empleado
            };

            _basedatos.ChatServer.Add(mensaje);

            await _basedatos.SaveChangesAsync();

            return Ok("{\"result\": \"Mensaje insertado exitosamente.\"}");
        }

        [HttpPost("ActualizarView")]
        public async Task<ActionResult> ActualizarView(int idCliente, string idEmpleado)
        {

            var cliente = await _basedatos.Clientes
                                        .FirstOrDefaultAsync(c => c.IdPersona == idCliente);
            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var empleado = await _basedatos.Empleados
                                          .FirstOrDefaultAsync(e => e.IdUsuario == idEmpleado);
            if (empleado == null)
            {
                return NotFound("{\"result\": \"Empleado no encontrado.\"}");
            }

            var mensajes = await _basedatos.ChatServer
                                           .Where(m => m.IdCliente == cliente.IdCliente && m.IdEmpleado == empleado.IdEmpleado)
                                           .ToListAsync();

            if (!mensajes.Any())
            {
                return NotFound("{\"result\": \"No se encontraron mensajes con los criterios especificados.\"}");
            }

            foreach (var mensaje in mensajes)
            {
                mensaje.View = true;
            }
            await _basedatos.SaveChangesAsync();

            return Ok("{\"result\": \"Todos los mensajes actualizados exitosamente.\"}");
        }

        [HttpGet("ObtenerMensajes")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> ObtenerMensajes(int idCliente, String idUsuario)
        {
            var persona = await _basedatos.Empleados
                                           .Where(m => m.IdUsuario == idUsuario)
                                           .FirstAsync();

            // Filtrar los mensajes por IdCliente y IdEmpleado
            var mensajes = await _basedatos.ChatServer
                                           .Where(m => m.IdCliente == idCliente && m.IdEmpleado == persona.IdEmpleado)
                                           .ToListAsync();

            if (!mensajes.Any())
            {
                return NotFound("{\"result\": \"No se encontraron mensajes con los criterios especificados.\"}");
            }

            // Mapear los resultados a un DTO para ser enviado como respuesta
            var mensajesDto = mensajes.Select(m => new MessageDto
            {
                Message = m.Message,
                Send = m.Send,
                View = m.View
            }).ToList();

            return Ok(mensajesDto);
        }

    }
}
