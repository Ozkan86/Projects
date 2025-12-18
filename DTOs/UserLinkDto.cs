using System;
using System.Collections.Generic;

namespace Eventify.DTOs
{
    public class UserLinkDto
    {
        public int Id { get; set; } // Kullanıcının ID'si
        public string Name { get; set; } = string.Empty; // Kullanıcı adı
        public string Email { get; set; } = string.Empty; // Kullanıcının e-posta adresi
        public string Role { get; set; } = string.Empty; // Kullanıcının rolü (örneğin: Admin, User) ////////
        public List<LinkDto> Links { get; set; } = new List<LinkDto>(); // HATEOAS linkleri listesi
    }
}
