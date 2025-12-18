using System;
using System.Collections.Generic;

namespace Eventify.DTOs
{
    public class EventLinkDto
    {
        public int Id { get; set; } // Etkinliğin ID'si
        public string Name { get; set; } = string.Empty; // Etkinlik adı
        public string Location { get; set; } = string.Empty; // Etkinlik lokasyonu
        public DateTime Date { get; set; } // Etkinlik tarihi
        public List<AttendanceDto>? Attendances { get; set; } // Etkinliğe katılanların listesi

        public List<LinkDto> Links { get; set; } = new List<LinkDto>(); // HATEOAS linkleri listesi
    }
}
