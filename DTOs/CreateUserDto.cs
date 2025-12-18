using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Eventify.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string Username { get; set; } // Kullanıcı adı

        [Required] // Bu alanın zorunlu olduğunu belirtir
        [EmailAddress] // E-posta adresinin geçerli formatta olup olmadığını kontrol eder
        public string Email { get; set; } // Kullanıcının e-posta adresi

        [Required]
        public string Password { get; set; } // Kullanıcının şifresi

        [Required]
        [StringLength(50)] // Kullanıcı adının maksimum uzunluğu 50 karakter
        public string Role { get; set; } // Kullanıcı rolü (ör: Admin, User)
    }
}
