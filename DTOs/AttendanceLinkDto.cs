using System;
using System.Collections.Generic;

namespace Eventify.DTOs
{
    public class AttendanceLinkDto
    {
        public int Id { get; set; } // Katılım kaydının ID'si
        public SimpleEventDto Event { get; set; } // İlgili etkinliğin temel bilgileri
        public SimpleUserDto User { get; set; } // İlgili kullanıcının temel bilgileri
        public bool IsAttending { get; set; } // Katılım durumu (true: katıldı, false: katılmadı)
        public List<LinkDto> Links { get; set; } = new List<LinkDto>(); // HATEOAS linkleri listesi
    }
}
