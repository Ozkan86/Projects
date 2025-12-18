using System.ComponentModel.DataAnnotations; // Required ve StringLength özelliklerini içeren kütüphane

namespace Eventify.DTOs
{
    public class UpdateEventDto
    {
        
        [Required]
        [StringLength(100)] // Etkinlik adı (en fazla 100 karakter)
        public string Name { get; set; } // Etkinlik adı (en fazla 100 karakter)

        [Required] 
        [StringLength(200)] // Etkinlik yeri (en fazla 200 karakter)
        public string Location { get; set; } // Etkinlik lokasyonu (en fazla 200 karakter)

        [Required]
        public DateTime Date { get; set; } // Etkinlik tarihi
    }
}
