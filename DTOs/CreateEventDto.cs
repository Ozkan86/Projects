using System.ComponentModel.DataAnnotations;

namespace Eventify.DTOs
{
    public class CreateEventDto
    {
        [Required(ErrorMessage = "Etkinlik adı gereklidir.")] // Boş bırakılamaz
        [StringLength(100, ErrorMessage = "Etkinlik adı en fazla 100 karakter olabilir.")] // Maksimum 100 karakter
        public string Name { get; set; } = string.Empty; // Etkinlik adı

        [Required(ErrorMessage = "Lokasyon gereklidir.")] // Boş bırakılamaz
        [StringLength(200, ErrorMessage = "Lokasyon en fazla 200 karakter olabilir.")] // Maksimum 200 karakter
        public string Location { get; set; } = string.Empty; // Etkinlik lokasyonu

        [Required(ErrorMessage = "Tarih gereklidir.")] // Boş bırakılamaz
        public DateTime Date { get; set; } // Etkinlik tarihi
    }
}
