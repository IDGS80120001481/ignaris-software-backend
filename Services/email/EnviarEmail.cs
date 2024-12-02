using LignarisBack.Models;
using System.Net;
using System.Net.Mail;

namespace LignarisBack.Services.email
{
    public class EnviarEmail
    {
        public bool EnviarSugerencias(Recetum receta, String email)
        {

            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("cordovagames012@gmail.com", "kbpn jgau scvv uzvv"),
                    EnableSsl = true
                };

                // Plantilla del correo electronico
                PlantillaSugerencias template = new PlantillaSugerencias();
                String htmlTemplate = template.templateSuggest();
                htmlTemplate = htmlTemplate.Replace("[NOMBRE]", receta.Nombre);
                htmlTemplate = htmlTemplate.Replace("[FOTO]", receta.Foto);
                htmlTemplate = htmlTemplate.Replace("[PRECIO]", receta.PrecioUnitario.ToString());

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("cordovagames012@gmail.com"),
                    Subject = "Lignaris Sugerencias",
                    Body = htmlTemplate,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex) {
                return false;
            }
        }

        public bool EnviarCarrito(List<Recetum> recetas, List<CarritoCompras> carritos, String email)
        {

            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("cordovagames012@gmail.com", "kbpn jgau scvv uzvv"),
                    EnableSsl = true
                };

                String body = "";
                // Plantilla del correo electronico
                PlantillaCarrito template = new PlantillaCarrito();
                for(var i = 0; i < carritos.Count; i++) 
                {
                    String htmlTemplate = template.TemplateProduct();
                    htmlTemplate = htmlTemplate.Replace("[FOTO]", recetas[i].Foto);
                    htmlTemplate = htmlTemplate.Replace("[NOMBRE]", recetas[i].Nombre);
                    htmlTemplate = htmlTemplate.Replace("[CANTIDAD]", carritos[i].Cantidad.ToString());
                    htmlTemplate = htmlTemplate.Replace("[PRECIO]", recetas[i].PrecioUnitario.ToString());
                    double? total = carritos[i].Cantidad * recetas[i].PrecioUnitario;
                    htmlTemplate = htmlTemplate.Replace("[TOTAL]", total.ToString());
                    body += htmlTemplate;
                }

                String htmlTemplateCart = template.TemplateCart();
                htmlTemplateCart = htmlTemplateCart.Replace("[BODY]", body);

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("cordovagames012@gmail.com"),
                    Subject = "Tienes productos disponibles en tu carrito de compras",
                    Body = htmlTemplateCart,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
