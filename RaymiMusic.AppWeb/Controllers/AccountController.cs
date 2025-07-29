using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaymiMusic.Api.Data;
using RaymiMusic.AppWeb.Models.Auth;
using RaymiMusic.AppWeb.Services;
using RaymiMusic.Modelos;

namespace RaymiMusic.AppWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _ctx;
        private readonly IEmailSender _emailSender;

        public AccountController(AppDbContext ctx, IEmailSender emailSender)
        {
            _ctx = ctx;
            _emailSender = emailSender;
        }

        // GET /Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST /Account/Register
        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (_ctx.Usuarios.Any(u => u.Correo == vm.Email))
            {
                ModelState.AddModelError("", "El correo ya está registrado");
                return View(vm);
            }

            // 1) Crear usuario Free
            var planFreeId = _ctx.Planes.First(p => p.Nombre == "Free").Id;
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Correo = vm.Email,
                HashContrasena = Hash(vm.Password),
                Rol = Roles.Free,
                PlanSuscripcionId = planFreeId
            };

            _ctx.Usuarios.Add(usuario);
            string? nombreArchivo = null;

            if (vm.FotoPerfil != null && vm.FotoPerfil.Length > 0)
            {
                // Crear nombre único
                var extension = Path.GetExtension(vm.FotoPerfil.FileName);
                nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var ruta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "usuarios", nombreArchivo);

                // Crear carpeta si no existe
                var directorio = Path.GetDirectoryName(ruta);
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);

                // Guardar archivo
                using (var stream = new FileStream(ruta, FileMode.Create))
                {
                    await vm.FotoPerfil.CopyToAsync(stream);
                }

                // Guardar la URL en el usuario
                usuario.UrlFotoPerfil = nombreArchivo;
            }

            await _ctx.SaveChangesAsync();

            // 2) Generar token de verificación
            var confirm = new EmailConfirmation
            {
                UsuarioId = usuario.Id,
                Token = Guid.NewGuid().ToString("N"),
                Expiration = DateTime.UtcNow.AddHours(24),
                IsConfirmed = false,
                Purpose = ConfirmationPurpose.EmailVerification
            };
            _ctx.EmailConfirmations.Add(confirm);
            await _ctx.SaveChangesAsync();

            // 3) Enviar correo de verificación
            var linkHtml = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { token = confirm.Token },
                Request.Scheme);
            var html = $"Para activar tu cuenta haz clic <a href=\"{linkHtml}\">aquí</a>.";
            await _emailSender.SendEmailAsync(vm.Email, "Confirma tu cuenta", html);

            return RedirectToAction(nameof(RegisterConfirmation));
        }

        [HttpGet]
        public IActionResult RegisterConfirmation() => View();

        // GET /Account/ConfirmEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token)) return View("ConfirmFailed");

            var rec = await _ctx.EmailConfirmations
                .FirstOrDefaultAsync(x =>
                    x.Token == token &&
                    x.Purpose == ConfirmationPurpose.EmailVerification &&
                    x.Expiration > DateTime.UtcNow);

            if (rec == null) return View("ConfirmFailed");

            rec.IsConfirmed = true;
            await _ctx.SaveChangesAsync();
            return View("ConfirmSuccess");
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _ctx.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == vm.Email);

            if (user == null || Hash(vm.Password) != user.HashContrasena)
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View(vm);
            }

            // Verificar que el email está confirmado
            var ok = await _ctx.EmailConfirmations
                .AnyAsync(c => c.UsuarioId == user.Id
                            && c.Purpose == ConfirmationPurpose.EmailVerification
                            && c.IsConfirmed);
            if (!ok)
            {
                ModelState.AddModelError("",
                    "Debes confirmar tu correo antes de iniciar sesión.");
                return View(vm);
            }

            // Crear claims y firmar
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.Correo),
                new Claim(ClaimTypes.Role,           user.Rol)
            };
            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                });
            // Redirección por rol

            switch (user.Rol.ToLower())
            {
                case "artista":
                    return RedirectToAction("Dashboard", "Dashboard", new { id = user.Id });
                case "free":
                case "premium":
                    return RedirectToAction("Index", "Home");
                case "admin":
                    return RedirectToAction("Index", "Home");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        // POST /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        // GET /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST /Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return View();

            var user = await _ctx.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            var reset = new EmailConfirmation
            {
                UsuarioId = user.Id,
                Token = Guid.NewGuid().ToString("N"),
                Expiration = DateTime.UtcNow.AddHours(2),
                IsConfirmed = false,
                Purpose = ConfirmationPurpose.PasswordReset
            };
            _ctx.EmailConfirmations.Add(reset);
            await _ctx.SaveChangesAsync();

            var link = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { token = reset.Token },
                Request.Scheme);
            var body = $"Para restablecer tu contraseña haz clic <a href=\"{link}\">aquí</a>.";
            await _emailSender.SendEmailAsync(email, "Restablece tu contraseña", body);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        // GET /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token)) return BadRequest();
            return View(new ResetPasswordVM { Token = token });
        }

        // POST /Account/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var rec = await _ctx.EmailConfirmations
                .FirstOrDefaultAsync(x =>
                    x.Token == vm.Token &&
                    x.Purpose == ConfirmationPurpose.PasswordReset &&
                    x.Expiration > DateTime.UtcNow);
            if (rec == null) return View("ResetFailed");

            var user = await _ctx.Usuarios.FindAsync(rec.UsuarioId);
            user.HashContrasena = Hash(vm.NewPassword);
            rec.IsConfirmed = true;
            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(ResetSuccess));
        }

        [HttpGet]
        public IActionResult ResetSuccess() => View();

        // Helpers
        private static string Hash(string plain)
        {
            using var sha = SHA256.Create();
            var buf = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
            return Convert.ToBase64String(buf);
        }

        // GET /Account/RegisterArtist
        [HttpGet]
        public IActionResult RegisterArtist() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterArtist(RegisterArtistVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (_ctx.Usuarios.Any(u => u.Correo == vm.Email))
            {
                ModelState.AddModelError("", "El correo ya está registrado");
                return View(vm);
            }

            // 1) Crear usuario con rol Artista
            var planFreeId = _ctx.Planes.First(p => p.Nombre == "Free").Id;
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Correo = vm.Email,
                HashContrasena = Hash(vm.Password),
                Rol = Roles.Artista,
                PlanSuscripcionId = planFreeId
            };
            _ctx.Usuarios.Add(usuario);

            await _ctx.SaveChangesAsync();

            // 2) Crear registro en Artistas (sin UsuarioId)
            // 2) Crear registro en Artistas, usando el mismo Id que el Usuario
            var artista = new Artista
            {
                Id = usuario.Id,                    // ← aquí
                NombreArtistico = vm.NombreArtistico,
                Biografia = vm.Biografia,
                UrlFotoPerfil = vm.UrlFotoPerfil,
                UrlFotoPortada = vm.UrlFotoPortada

                
            };

            _ctx.Artistas.Add(artista);
            await _ctx.SaveChangesAsync();

            // 3) Generar token de verificación
            var confirm = new EmailConfirmation
            {
                UsuarioId = usuario.Id,
                Token = Guid.NewGuid().ToString("N"),
                Expiration = DateTime.UtcNow.AddHours(24),
                IsConfirmed = false,
                Purpose = ConfirmationPurpose.EmailVerification
            };
            _ctx.EmailConfirmations.Add(confirm);
            await _ctx.SaveChangesAsync();

            // 4) Enviar correo de verificación
            var link = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { token = confirm.Token },
                Request.Scheme);
            var html = $@"
        <p>Hola <strong>{vm.NombreArtistico}</strong>,</p>
        <p>Para activar tu cuenta de artista haz clic <a href=""{link}"">aquí</a>.</p>";
            await _emailSender.SendEmailAsync(vm.Email, "Confirma tu cuenta de artista", html);

            // 5) Redirigir a confirmación
            return RedirectToAction(nameof(RegisterArtistConfirmation));
        }


        // GET /Account/RegisterArtistConfirmation
        [HttpGet]
        public IActionResult RegisterArtistConfirmation(string email)
        {
            ViewData["Email"] = email;
            return View();
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


    }
}
