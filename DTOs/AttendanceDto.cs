namespace Eventify.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; } // Katılım kaydının ID'si
        public bool IsAttending { get; set; } // Katılım durumu (true: katıldı, false: katılmadı)
        public SimpleUserDto? User { get; set; } // Katılan kullanıcının temel bilgileri
    }
}
