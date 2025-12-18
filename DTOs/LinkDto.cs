namespace Eventify.DTOs
{
    public class LinkDto
    {
        public string Href { get; set; } = string.Empty;  // Bağlantının URL'si
        public string Rel { get; set; } = string.Empty;   // Bağlantının ilişkisi (ör: self, update)
        public string Method { get; set; } = string.Empty; // HTTP metodu (GET, POST, PUT, DELETE)
    }
}
