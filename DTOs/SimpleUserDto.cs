namespace Eventify.DTOs
{
    public class SimpleUserDto
    {
        public int Id { get; set; } // Kullanıcının ID'si
        public string Username { get; set; } = null!; // Kullanıcı adı
        public string Email { get; set; } = null!; // Kullanıcının e-posta adresi
        public string Role { get; set; } = null!; // Kullanıcı rolü (ör: Admin, User)
    }
}