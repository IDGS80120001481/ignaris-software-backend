using LignarisBack.Dto;
using LignarisBack.Models;
using LignarisBack.Dto;
using LignarisBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace LignarisBack.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly LignarisPizzaContext _basedatos;

        public UsuariosController(UserManager<AppUser> userManager, RoleManager<IdentityRole>
       roleManager, IConfiguration configuration, LignarisPizzaContext basedatos)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _basedatos = basedatos;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var person = new Persona
            {
                Nombre = registerDto.Nombre,
                ApellidoPaterno = registerDto.ApellidoPaterno,
                ApellidoMaterno = registerDto.ApellidoMaterno,
                Telefono = registerDto.Telefono,
                Direccion = registerDto.Direccion,
                Email = registerDto.Email
            };

            _basedatos.Personas.Add(person);
            await _basedatos.SaveChangesAsync();

            int idPersona = person.IdPersona;

            var code = new Random().Next(100000, 999999).ToString();
            var expirationTime = DateTime.UtcNow.AddMinutes(10);
            var user = new AppUser
            {
                Email = registerDto.Email,
                Fullname = registerDto.Nombre + " " + registerDto.ApellidoPaterno,
                UserName = registerDto.Email,
                Code = code,
                ExpirationCode = expirationTime,
                AccountActive = 0,
                TokenActivate = ""
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            EnviarCorreoCode(user.Email, code);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            if (registerDto.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, "Cliente");
            }
            else
            {
                foreach (var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            var idUsuario = user.Id;
            var cliente = new Cliente
            {
                IdUsuario = idUsuario,
                IdPersona = idPersona,
            };
            _basedatos.Clientes.Add(cliente);
            await _basedatos.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Cuenta creada exitosamente."
            });
        }

        [AllowAnonymous]
        [HttpPost("register_empleado")]
        public async Task<ActionResult<string>> RegisterEmpleado(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var person = new Persona
            {
                Nombre = registerDto.Nombre,
                ApellidoPaterno = registerDto.ApellidoPaterno,
                ApellidoMaterno = registerDto.ApellidoMaterno,
                Telefono = registerDto.Telefono,
                Direccion = registerDto.Direccion,
                Email = registerDto.Email
            };

            _basedatos.Personas.Add(person);
            await _basedatos.SaveChangesAsync();
            int idPersona = person.IdPersona;

            var code = new Random().Next(100000, 999999).ToString();
            var expirationTime = DateTime.UtcNow.AddMinutes(10);
            var user = new AppUser
            {
                Email = registerDto.Email,
                Fullname = registerDto.Nombre + " " + registerDto.ApellidoPaterno,
                UserName = registerDto.Email,
                Code = code,
                ExpirationCode = expirationTime,
                AccountActive = 0 ,
                TokenActivate = ""
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            EnviarCorreoCode(user.Email, code);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            if (registerDto.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, "Empleado");
            }
            else
            {
                foreach (var role in registerDto.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            var idUsuario = user.Id;
            var empleado = new Empleado
            {
                IdUsuario = idUsuario,
                IdPersona = idPersona,
                Puesto = "Vendedor"
            };
            _basedatos.Empleados.Add(empleado);
            await _basedatos.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Cuenta creada exitosamente."
            });
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado con este correo."
                });
            }

            if (user.AccountActive == 0)
            {
                var tokenActivate = GenerateToken();
                SaveToken(user.Id, tokenActivate);
                EnviarCorreo(user.Email, user.TokenActivate);

                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario desactivado por favor ingresa a tu correo para volver activar tu cuenta."
                });
            }

            var lastSession = user.LastSession;
            user.LastSession = DateTime.Now;
            await _userManager.UpdateAsync(user);

            string role = "";
            var empleado = await _basedatos.Empleados.Where(v => v.IdUsuario == user!.Id).ToListAsync();

            if (empleado.Any())
            {
                role = "Empleado";
            }
            else
            {
                var cliente = await _basedatos.Clientes.Where(c => c.IdUsuario == user!.Id).ToListAsync();

                if (cliente.Any())
                {
                    role = "Cliente";
                }
                else
                {
                    return BadRequest("El usuario no está registrado.");
                }
            }


            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                user.FailedAttempts++;
                await _userManager.UpdateAsync(user);
                if (user.FailedAttempts > 3)
                {
                    user.FailedAttempts++;
                    user.AccountActive = 0;
                    var tokenActivate = GenerateToken();
                    SaveToken(user.Id, tokenActivate);
                    EnviarCorreo(user.Email, user.TokenActivate);
                    await _userManager.UpdateAsync(user);
                }
                await _userManager.UpdateAsync(user);

                return Unauthorized(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Contraseña incorrecta"
                });
            }
            var token = GenerateToken(user);
            user.FailedAttempts = 0;
            await _userManager.UpdateAsync(user);
            return Ok(new AuthResponseDto
            {
                Token = token,
                IsSuccess = true,
                Message = "Acceso correcto",
                Role = role,
                LastSession = lastSession
            });
        }

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        private async void SaveToken(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            user.TokenActivate = token;
            user.ExpirationTime = DateTime.UtcNow.AddHours(1);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Error al actualizar el usuario.");
            }
        }

        private void EnviarCorreo(String email, String token)
        {
            var link = $"https://localhost:5000/api/Usuarios/activar-cuenta?token={token}";

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("cordovagames012@gmail.com", "kbpn jgau scvv uzvv"),
                EnableSsl = true
            };

            // Crear el mensaje
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress("cordovagames012@gmail.com"),
                Subject = "Reactiva tu cuenta, numerosos Intentos fallidos",
                Body = $"Para volver activar tu cuenta por favor ingresa a este link, de lo contrario no podras acceder a tu cuenta   <a href='{link}'>Activar cuenta</a>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            try
            {
                smtpClient.Send(mailMessage);
                Console.WriteLine("Correo enviado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar correo: " + ex.Message);
            }
        }

        [HttpGet("code-activate")]
        [AllowAnonymous]
        public async Task<ActionResult> GenerateVerificationCode([FromQuery] string email, [FromQuery] string code)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            if (user.Code != code)
            {
                return BadRequest(new { message = "El código ha expirado o es inválido" });
            }

            user.AccountActive = 1;
            user.Code = "0"; // Limpiamos el código
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Cuenta validada correctamente" });
        }


        private void EnviarCorreoCode(String email, String code)
        {

            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("cordovagames012@gmail.com", "kbpn jgau scvv uzvv"),
                EnableSsl = true
            };

            // Crear el mensaje
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress("cordovagames012@gmail.com"),
                Subject = "Activa tu cuenta",
                Body = $"Para activar tu cuenta por favor ingresa este codigo en la aplicacion para validar que tu has realizado la solicitud {code}",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            try
            {
                smtpClient.Send(mailMessage);
                Console.WriteLine("Correo enviado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar correo: " + ex.Message);
            }
        }

        [HttpGet("activar-cuenta")]
        [AllowAnonymous]
        public async Task<ActionResult> ActivarCuenta(string token)
        {
            var tokenEntry = await _basedatos.AppUser
                                            .Where(t => t.TokenActivate == token)
                                            .FirstOrDefaultAsync();

            if (tokenEntry == null || tokenEntry.ExpirationTime < DateTime.UtcNow)
            {
                return BadRequest("El token ha expirado o es inválido.");
            }

            var user = await _userManager.FindByIdAsync(tokenEntry.Id);

            if (user == null)
            {
                return Unauthorized("Usuario no encontrado, regresa a la aplicacion y vuelve a acceder a tu cuenta.");
            }

            // Lógica de activación, por ejemplo, activar la cuenta
            user.AccountActive = 1;
            await _userManager.UpdateAsync(user);

            return Ok("Cuenta activada con éxito.");
        }

        private string GenerateToken(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key =
           Encoding.ASCII.GetBytes(_configuration.GetSection("JWTSetting").GetSection("securityKey").Value!);
            var roles = _userManager.GetRolesAsync(user).Result;
            List<Claim> claims = [
            new (JwtRegisteredClaimNames.Email, user.Email??""),
            new (JwtRegisteredClaimNames.Name, user.Fullname??""),
            new (JwtRegisteredClaimNames.NameId, user.Id??""),
            new (JwtRegisteredClaimNames.Aud,
            _configuration.GetSection("JWTSetting").GetSection("ValidAudience").Value!),
            new (JwtRegisteredClaimNames.Iss,
            _configuration.GetSection("JWTSetting").GetSection("ValidIssuer").Value!)
            ];
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
           SecurityAlgorithms.HmacSha256
            )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        //[Authorize]
        [HttpGet("detail")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);
            var persona = await _basedatos.Personas.FindAsync(user!.Id);
            if (user == null)
            {
                return NotFound(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario no encontrado"
                });
            }

            var personadto = new PersonaDto
            {
                Nombre = persona!.Nombre,
                ApellidoPaterno = persona.ApellidoPaterno,
                ApellidoMaterno = persona.ApellidoMaterno,
                Direccion = persona.Direccion,
                Email = persona.Email,
                Telefono = persona.Telefono
            };

            return Ok(new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.Fullname,
                Roles = [.. await _userManager.GetRolesAsync(user)],
                Persona = personadto
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var users = await _userManager.Users.Select(u => new UserDetailDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.Fullname,
                Roles = _userManager.GetRolesAsync(u).Result.ToArray()
            }).ToListAsync();
            return Ok(users);
        }

    }
}
