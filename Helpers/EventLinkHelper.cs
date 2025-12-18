using Eventify.DTOs;

public static class EventLinkHelper
{
    public static List<LinkDto> GenerateLinks(int id, string baseUrl)
    {
        return new List<LinkDto> // LinkDto nesneleri içeren bir liste döndürür
        {
            new LinkDto
            {
                Href = $"{baseUrl}/api/v2/events/{id}", // Etkinliğin detayına giden bağlantı
                Rel = "self", // Kendi kaynağı
                Method = "GET" // HTTP GET metodu
            },
            new LinkDto
            {
                Href = $"{baseUrl}/api/v2/events/{id}", // Etkinliği güncelleme bağlantısı
                Rel = "update_event", // Güncelleme işlemi
                Method = "PUT" // HTTP PUT metodu
            },
            new LinkDto
            {
                Href = $"{baseUrl}/api/v2/events/{id}", // Etkinliği silme bağlantısı
                Rel = "delete_event", // Silme işlemi
                Method = "DELETE" // HTTP DELETE metodu
            }
        };
    }
}
