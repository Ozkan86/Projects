using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eventify.DTOs;
using Eventify.Services;
using Asp.Versioning;
using Serilog;

namespace Eventify.Controllers.V2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config; // Uygulama ayarlarını okur
        private readonly IUserService _userService; // Kullanıcı işlemleri servisi

        public AuthController(IConfiguration config, IUserService userService) // Kurucu enjeksiyonu
        {
            _config = config; // Konfigürasyon nesnesi atanır
            _userService = userService; // Kullanıcı servisi atanır
        }

        [HttpPost("login")] // POST /login endpoint'i
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) // Model doğrulaması
                return BadRequest(ModelState); // Model doğrulama hatası varsa 400 döner

            var user = await _userService.AuthenticateAsync(loginDto.Username, loginDto.Password); // Kullanıcı doğrulama

            if (user == null)
                return Unauthorized(new { message = "Kullanıcı adı veya şifre hatalı" }); // Hatalı girişte 401 döner

            var claims = new[] // Talep tipleri ve değerleri oluşturuluyor
            {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcı ID'si claim'i
                            new Claim(ClaimTypes.Name, user.Username), // Kullanıcı adı claim'i
                            new Claim(ClaimTypes.Role, user.Role), // Kullanıcı rolü claim'i
                        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])); // JWT anahtarı (veri şifreleme)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // İmzalama algoritması

            var token = new JwtSecurityToken( // Token oluşturuluyor
                issuer: _config["Jwt:Issuer"], // JWT yayıncısı
                audience: _config["Jwt:Audience"], // JWT alıcısı
                claims: claims, // Token'a eklenecek claim'ler
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresInMinutes"])), // Token süresi
                signingCredentials: creds); // İmzalama bilgisi

            Log.Information("Kullanıcı giriş yaptı. Kullanıcı adı: {Username}, Rol: {Role}", user.Username, user.Role); // Loglama yapıldı
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token) // JWT token'ı döner
            });
        }
    }
}
