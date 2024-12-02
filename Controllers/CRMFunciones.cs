using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using MailKit.Search;
using MailKit.Net.Imap;
using MimeKit;

namespace LignarisBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CRMFunciones : ControllerBase
    {

        private readonly LignarisPizzaContext _basedatos;

        public CRMFunciones(UserManager<AppUser> userManager, RoleManager<IdentityRole>
       roleManager, IConfiguration configuration, LignarisPizzaContext basedatos)
        {
            _basedatos = basedatos;
        }

        [HttpGet]
        [Route("getclients")]
        [Authorize(Roles = "Empleado")]
        public async Task<ActionResult<IEnumerable<PersonaDto>>> GetEmpleados()
        {
            var personasDto = new List<PersonaDto>();
            var idsPersona = await _basedatos.Clientes.Select(e => e.IdPersona).ToListAsync();

            foreach (var id in idsPersona)
            {
                var persona = await _basedatos.Personas.FirstOrDefaultAsync(p => p.IdPersona == id);

                if (persona != null)
                {
                    var personaDto = MapPersonaDto(persona);
                    personasDto.Add(personaDto);
                }
                else
                {
                    Console.WriteLine($"No se encontró una persona con Id {id}");
                }
            }
            return Ok(personasDto);
        }

        [HttpPost]
        [Route("send")]
        public IActionResult SendEmail([FromBody] EmailRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Message) || string.IsNullOrEmpty(request.Subject))
            {
                return BadRequest("Todos los campos son obligatorios.");
            }

            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("cordovagames012@gmail.com", "kbpn jgau scvv uzvv"),
                    EnableSsl = true
                };

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("cordovagames012@gmail.com"),
                    Subject = request.Subject,
                    Body = request.Message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(request.Email);

                smtpClient.Send(mailMessage);
                return Ok("Correo enviado exitosamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al enviar correo: " + ex.Message);
            }
        }

        [HttpPost]
        [Route("emails")]
        public IActionResult GetEmails([FromBody] SearchEmailDtoDto request)
        {
            if (string.IsNullOrEmpty(request.Request))
            {
                return BadRequest("La dirección de correo es requerida.");
            }

            try
            {
                List<EmailInfoDto> emails = new List<EmailInfoDto>();

                using (var client = new ImapClient())
                {
                    client.Connect("imap.gmail.com", 993, true);
                    client.Authenticate("cordovagames012@gmail.com", "kbpn jgau scvv uzvv");
                    var sentFolder = client.GetFolder("[Gmail]/Enviados");
                    sentFolder.Open(MailKit.FolderAccess.ReadOnly);
                    var sentQuery = SearchQuery.ToContains(request.Request);
                    var sentEmails = sentFolder.Search(sentQuery);

                    foreach (var uniqueId in sentEmails)
                    {
                        var message = sentFolder.GetMessage(uniqueId);
                        string body = GetBodyContent(message);
                        emails.Add(new EmailInfoDto
                        {
                            Subject = message.Subject,
                            To = string.Join(", ", message.To),
                            From = string.Join(", ", message.From),
                            Date = message.Date.DateTime,
                            Body = body,
                            Folder = "Enviados"
                        });
                    }

                    var inboxFolder = client.Inbox;
                    inboxFolder.Open(MailKit.FolderAccess.ReadOnly);
                    var receivedQuery = SearchQuery.FromContains(request.Request);
                    var receivedEmails = inboxFolder.Search(receivedQuery);

                    foreach (var uniqueId in receivedEmails)
                    {
                        var message = inboxFolder.GetMessage(uniqueId);
                        string body = GetBodyContent(message);
                        emails.Add(new EmailInfoDto
                        {
                            Subject = message.Subject,
                            To = string.Join(", ", message.To),
                            From = string.Join(", ", message.From),
                            Date = message.Date.DateTime,
                            Body = body,
                            Folder = "Recibidos"
                        });
                    }
                    client.Disconnect(true);
                }
                var orderedEmails = emails.OrderByDescending(e => e.Date).ToList();
                return Ok(orderedEmails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al obtener correos: " + ex.Message);
            }
        }

        private string GetBodyContent(MimeMessage message)
        {
            var textPart = message.TextBody;
            var htmlPart = message.HtmlBody;

            if (!string.IsNullOrEmpty(htmlPart))
            {
                return htmlPart;
            }
            return !string.IsNullOrEmpty(textPart) ? textPart : "No hay cuerpo de mensaje";
        }

        [HttpGet("ObtenerCliente/{idPersona}")]
        public async Task<ActionResult> ObtenerCliente(int idPersona)
        {
            // Validar entrada
            if (idPersona <= 0)
            {
                return BadRequest("{\"result\": \"IdPersona no válido.\"}");
            }

            // Buscar cliente por IdPersona
            var cliente = await _basedatos.Clientes
                                          .FirstOrDefaultAsync(c => c.IdPersona == idPersona);

            // Validar si el cliente existe
            if (cliente == null)
            {
                return NotFound("{\"result\": \"Cliente no encontrado.\"}");
            }

            // Crear la respuesta
            var resultado = new
            {
                cliente.IdCliente,
                cliente.IdPersona,
                cliente.IdUsuario,
                cliente.TelefonoFijo,
                cliente.Linkedin,
                cliente.Youtube,
                cliente.Facebook,
                cliente.Twitter,
                cliente.ComoLlego
            };

            return Ok(resultado);
        }

        private static PersonaDto MapPersonaDto(Persona persona) =>
           new PersonaDto
           {
               IdPersona = persona.IdPersona,
               Nombre = persona.Nombre,
               ApellidoPaterno = persona.ApellidoPaterno,
               ApellidoMaterno = persona.ApellidoMaterno,
               Direccion = persona.Direccion,
               Telefono = persona.Telefono,
               Email = persona.Email
           };
    }
}
