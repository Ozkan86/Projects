using System.ComponentModel.DataAnnotations; // Gereksinim duyulan alanları doğrulamak için gerekli kütüphaneler

namespace Eventify.DTOs
{
    public class UpdateUserDto
    {
        [Required] // Kullanıcı adı zorunlu alan
        public string Username { get; set; } // Kullanıcı adı

        [Required]
        [EmailAddress] // E-posta adresi doğrulama
        public string Email { get; set; } // Kullanıcının e-posta adresi

        [Required]
        public string Password { get; set; } // Kullanıcının şifresi

        [Required]
        [StringLength(50)] // Rol alanı için maksimum uzunluk
        public string Role { get; set; } // Kullanıcı rolü (ör: Admin, User)
    }
}
