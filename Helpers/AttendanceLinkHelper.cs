using Eventify.DTOs;

namespace Eventify.Helpers
{
    public static class AttendanceLinkHelper
    {
        public static List<LinkDto> GenerateLinks(int id, string baseUrl) // Katılım kaydı için bağlantıları oluşturur
        {
            return new List<LinkDto>
                {
                    new LinkDto
                    {
                        Href = $"{baseUrl}/api/v2/attendances/{id}", // Katılım kaydının detayına giden bağlantı
                        Rel = "self", // Kendi kaynağı
                        Method = "GET" // HTTP GET metodu
                    },
                    new LinkDto
                    {
                        Href = $"{baseUrl}/api/v2/attendances/{id}", // Katılım kaydını güncelleme bağlantısı
                        Rel = "update_attendance", // Güncelleme işlemi
                        Method = "PUT" // HTTP PUT metodu
                    },
                    new LinkDto
                    {
                        Href = $"{baseUrl}/api/v2/attendances/{id}", // Katılım kaydını silme bağlantısı
                        Rel = "delete_attendance", // Silme işlemi
                        Method = "DELETE" // HTTP DELETE metodu
                    }
                };
        }
    }
}
