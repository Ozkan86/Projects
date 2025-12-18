namespace Eventify.DTOs
{
    public class EventDto
    {
        public int Id { get; set; } // Etkinliğin ID'si
        public string Name { get; set; } = null!; // Etkinlik adı
        public string Location { get; set; } = null!; // Etkinlik lokasyonu
        public DateTime Date { get; set; } // Etkinlik tarihi

        public List<AttendanceDto>? Attendances { get; set; } // Etkinliğe katılanların listesi
    }
}
