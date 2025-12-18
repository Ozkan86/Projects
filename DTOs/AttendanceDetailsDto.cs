using Eventify.DTOs;

public class AttendanceDetailsDto
{
    public int Id { get; set; } // Katılım kaydının benzersiz ID'si
    public int EventId { get; set; } // İlgili etkinliğin ID'si
    public int UserId { get; set; } // İlgili kullanıcının ID'si
    public bool IsAttending { get; set; } // Katılım durumu (true: katılıyor, false: katılmıyor)

    public EventDto Event { get; set; } // İlgili etkinliğin detayları
    public SimpleUserDto User { get; set; } // İlgili kullanıcının temel bilgileri
}
