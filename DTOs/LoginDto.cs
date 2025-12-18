using System.ComponentModel.DataAnnotations; // Gereksinim duyulan veri doğrulama öznitelikleri için gerekli kütüphane

namespace Eventify.DTOs
{
    public class LoginDto
    {
        [Required] // Kullanıcı adı zorunlu alan
        public string Username { get; set; } = string.Empty; // Kullanıcı adı

        [Required] // Şifre zorunlu alan
        public string Password { get; set; } = string.Empty; // Kullanıcı şifresi
    }
}
