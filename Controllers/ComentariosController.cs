using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComentariosController : ControllerBase
    {
        private readonly LignarisPizzaContext _basedatos;

        public ComentariosController(LignarisPizzaContext basedatos)
        {
            _basedatos = basedatos;
        }


        [HttpPost]
        [Route("add")]
        public async Task<ActionResult<VentaDto>> InsertComentarios([FromBody] ComentariosDto comentarios)
        {
            var cliente = await _basedatos.Clientes
                                         .Where(c => c.IdUsuario!.Contains(comentarios.IdComentarios!))
                                         .FirstOrDefaultAsync();
            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var recetaExistente = await _basedatos.Receta.AnyAsync(r => r.IdReceta == comentarios.idReceta);

            if (!recetaExistente)
            {
                Console.WriteLine("La receta no existe.");
                return NotFound("{\"result\": \"No se encontro la receta del comentario.\"}");
            }
            else
            {
                var comentariosadd = new Comentarios
                {
                    IdCliente = cliente.IdCliente,
                    IdReceta = comentarios.idReceta,
                    TipoComentario = comentarios.TipoComentario,
                    Comentario = comentarios.Comentario
                };

                _basedatos.Comentarios.Add(comentariosadd);
                await _basedatos.SaveChangesAsync();
                return Ok(new { result = "Se ha realizado correctamente el comentario." });
            }
        }

        [HttpGet("commentsclient/{idCliente}")]
        public async Task<ActionResult<IEnumerable<ComentariosDto>>> GetComentariosPorCliente(int idCliente)
        {
            var cliente = await _basedatos.Clientes
                                         .Where(c => c.IdPersona == (idCliente))
                                         .FirstOrDefaultAsync();
            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            var comentarios = await _basedatos.Comentarios
                .Where(c => c.IdCliente == cliente.IdCliente)
                .Include(c => c.Cliente)
                .ToListAsync();

            if (comentarios == null || !comentarios.Any())
            {
                return NotFound();
            }

            var comentariosDto = comentarios.Select(comment => new ComentariosDto
            {
                IdComentarios = comment.IdComentarios.ToString(),
                idReceta = comment.IdCliente,
                TipoComentario = comment.TipoComentario,
                Comentario = comment.Comentario,
            }).ToList();

            return Ok(comentariosDto);
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<ComentariosDto>>> GetComentariosPorCategoria(string category)
        {

            var comentarios = await _basedatos.Comentarios
                .Where(c => c.TipoComentario == category)
                .ToListAsync();

            if (comentarios == null || !comentarios.Any())
            {
                return NotFound();
            }

            var clientesIds = comentarios.Select(c => c.IdCliente).ToList();
            var clientes = await _basedatos.Clientes
                .Where(c => clientesIds.Contains(c.IdCliente))
                .AsNoTracking()
                .ToListAsync();

            var usuariosIds = clientes.Select(c => c.IdUsuario).ToList();
            var usuarios = await _basedatos.AppUser
                .Where(a => usuariosIds.Contains(a.Id))
                .AsNoTracking()
                .ToListAsync();

            var comentariosDto = comentarios.Select(comment =>
            {
                var cliente = clientes.FirstOrDefault(c => c.IdCliente == comment.IdCliente);
                var usuario = usuarios.FirstOrDefault(u => u.Id == cliente?.IdUsuario);
                return new ComentariosDto
                {
                    IdComentarios = comment.IdComentarios.ToString(),
                    idReceta = comment.IdReceta,
                    TipoComentario = comment.TipoComentario,
                    Comentario = comment.Comentario,
                    Nombre = usuario?.Fullname,
                    FechaCreacion = comment.FechaCreacion
                };
            }).ToList();

            return Ok(comentariosDto);
        }

        [HttpGet()]
        [Route("comments")]
        public async Task<ActionResult<IEnumerable<ComentariosDto>>> GetComentarios()
        {

            var comentarios = await _basedatos.Comentarios
                .Take(150)
                .ToListAsync();

            if (comentarios == null || !comentarios.Any())
            {
                return NotFound();
            }

            var clientesIds = comentarios.Select(c => c.IdCliente).ToList();
            var clientes = await _basedatos.Clientes
                .Where(c => clientesIds.Contains(c.IdCliente))
                .AsNoTracking()
                .ToListAsync();

            var usuariosIds = clientes.Select(c => c.IdUsuario).ToList();
            var usuarios = await _basedatos.AppUser
                .Where(a => usuariosIds.Contains(a.Id))
                .AsNoTracking()
                .ToListAsync();

            var comentariosDto = comentarios.Select(comment =>
            {
                var cliente = clientes.FirstOrDefault(c => c.IdCliente == comment.IdCliente);
                var usuario = usuarios.FirstOrDefault(u => u.Id == cliente?.IdUsuario);
                return new ComentariosDto
                {
                    IdComentarios = comment.IdComentarios.ToString(),
                    idReceta = comment.IdReceta,
                    TipoComentario = comment.TipoComentario,
                    Comentario = comment.Comentario,
                    Nombre = usuario?.Fullname,
                    FechaCreacion = comment.FechaCreacion
                };
            }).ToList();

            return Ok(comentariosDto);
        }

        [HttpGet("commentsproduct/{idProducto}")]
        public async Task<ActionResult<IEnumerable<ComentariosDto>>> GetComentariosView(int idProducto)
        {
            var comentarios = await _basedatos.Comentarios
                .Where(c => c.IdReceta == idProducto)
                .AsNoTracking()
                .Take(10)
                .ToListAsync();

            if (comentarios == null || !comentarios.Any())
            {
                return NotFound();
            }

            var clientesIds = comentarios.Select(c => c.IdCliente).ToList();
            var clientes = await _basedatos.Clientes
                .Where(c => clientesIds.Contains(c.IdCliente))
                .AsNoTracking()
                .ToListAsync();

            var usuariosIds = clientes.Select(c => c.IdUsuario).ToList();
            var usuarios = await _basedatos.AppUser
                .Where(a => usuariosIds.Contains(a.Id))
                .AsNoTracking()
                .ToListAsync();

            var comentariosDto = comentarios.Select(comment =>
            {
                var cliente = clientes.FirstOrDefault(c => c.IdCliente == comment.IdCliente);
                var usuario = usuarios.FirstOrDefault(u => u.Id == cliente?.IdUsuario);
                return new ComentariosDto
                {
                    IdComentarios = comment.IdComentarios.ToString(),
                    idReceta = comment.IdReceta,
                    TipoComentario = comment.TipoComentario,
                    Comentario = comment.Comentario,
                    Nombre = usuario?.Fullname,
                    FechaCreacion = comment.FechaCreacion
                };
            }).ToList();

            return Ok(comentariosDto);
        }

    }
}
