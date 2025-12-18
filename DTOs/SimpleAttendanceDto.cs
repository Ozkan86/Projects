namespace Eventify.DTOs
{
    public class SimpleAttendanceDto
    {
        public int Id { get; set; } // Katılım kaydının ID'si
        public bool IsAttending { get; set; } // Katılım durumu (true: katıldı, false: katılmadı)
        public SimpleEventDto Event { get; set; } = null!; // İlgili etkinliğin temel bilgileri
        public SimpleUserDto User { get; set; } = null!; // İlgili kullanıcının temel bilgileri
    }
}
