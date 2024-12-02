using LignarisBack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net.Mail;
using System.Net;
using LignarisBack.Services.email;

namespace LignarisBack.Services
{
    public class EmailNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EmailNotificationService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Crear un alcance para el DbContext
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<LignarisPizzaContext>();

                    // Obtener la hora de las configuraciones
                    var horafix = await _context.HoraNotificacions.FirstAsync();
                    // Obtener la hora actual
                    DateTime horaActualtext = DateTime.Now;

                    // Convertimos la hora actual en texto y solo dos digitos
                    string horaActual = horaActualtext.ToString("HH:mm:ss").Substring(0, 5);
                    // Obtener la hora de envio notificaciones para el seguimiento del carrito
                    string horaCarrito = horafix.HoraCarrito.ToString().Substring(0, 5);
                    // Obtener la hora de envio de sugerencias
                    string horaSugerencias = horafix.HoraSugerencias.ToString().Substring(0, 5);
                    // Metodo que crea los correos electronicos
                    EnviarEmail mail = new EnviarEmail();

                    if (horaActual.Equals(horaCarrito))
                    {
                        var clienteIds = await _context.CarritoCompras
                            .GroupBy(c => c.IdCliente)
                            .Select(group => group.Key)
                            .ToArrayAsync();

                        foreach(var clienteId in clienteIds) 
                        {
                            var recetas = new List<Recetum>();
                            var cliente = await _context.Clientes.Where(c => c.IdCliente == clienteId).FirstAsync();
                            var persona = await _context.Personas.Where(p => p.IdPersona == cliente.IdPersona).FirstAsync();

                            var carritos = await _context.CarritoCompras.Where(c => c.IdCliente == cliente.IdCliente).ToListAsync();
                            foreach (var carrito in carritos)
                            {
                                var receta = await _context.Receta.Where(r => r.IdReceta == carrito.IdRecetas).FirstAsync();
                                recetas.Add(receta);
                            }
                            mail.EnviarCarrito(recetas, carritos, persona.Email);

                        }
                    }

                    if (horaActual.Equals(horaSugerencias))
                    {
                        var clientes = await _context.Personas.Include(c => c.Clientes).ToArrayAsync();
                        var receta = await _context.Receta.Where(c => c.suggest == true).FirstAsync();

                        foreach (var cliente in clientes) 
                        {
                           mail.EnviarSugerencias(receta, cliente.Email);
                        }
                    }
                }

                // Esperar 5 segundos antes de realizar la siguiente iteración
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}
