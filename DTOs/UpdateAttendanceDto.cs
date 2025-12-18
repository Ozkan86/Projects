using System.ComponentModel.DataAnnotations; // veri doğrulama için gerekli kütüphane

namespace Eventify.DTOs
{
    public class UpdateAttendanceDto
    {
        [Required] // Katılım kaydının ID'si zorunlu alan
        public int Id { get; set; } // Güncellenecek katılım kaydının ID'si

        [Required]
        public int EventId { get; set; } // Etkinlik ID'si

        [Required]
        public int UserId { get; set; } // Kullanıcı ID'si

        [Required]
        public bool IsAttending { get; set; } // Katılım durumu (true: katıldı, false: katılmadı)
    }
}
