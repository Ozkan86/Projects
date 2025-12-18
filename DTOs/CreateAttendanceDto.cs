using System.ComponentModel.DataAnnotations;

namespace Eventify.DTOs
{
    public class CreateAttendanceDto
    {
        [Required] //Bu alanın zorunlu olduğunu belirtir
        public int EventId { get; set; } // Katılım yapılacak etkinliğin ID'si

        [Required]
        public int UserId { get; set; } // Katılan kullanıcının ID'si

        [Required]
        public bool IsAttending { get; set; } // Katılım durumu (true: katıldı, false: katılmadı)
    }
}
