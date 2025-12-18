using Eventify.DTOs;

namespace Eventify.Helpers
{
    public static class UserLinkHelper
    {
        public static List<LinkDto> GenerateLinks(int id, string baseUrl) 
        {
            return new List<LinkDto> // LinkDto nesneleri içeren bir liste döndürür
                {
                    new LinkDto
                    {
                        Href = $"{baseUrl}/api/v2/users/{id}", // Kullanıcı detayına giden bağlantı
                        Rel = "self", // Kendi kaynağı
                        Method = "GET" // HTTP GET metodu
                    },
                    new LinkDto
                    {
                        Href = $"{baseUrl}/api/v2/users/{id}", // Kullanıcıyı güncelleme bağlantısı
                        Rel = "update_user", // Güncelleme işlemi
                        Method = "PUT" // HTTP PUT metodu
                    },
                    new LinkDto
                    {
                        Href = $"{baseUrl}/api/v2/users/{id}", // Kullanıcıyı silme bağlantısı
                        Rel = "delete_user", // Silme işlemi
                        Method = "DELETE" // HTTP DELETE metodu
                    }
                };
        }
    }
}
