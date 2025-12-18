using Eventify.DTOs;

namespace Eventify.Services
{
    public interface IUserService
    {

        Task<IEnumerable<SimpleUserDto>> GetAllUsersAsync(UserQueryParametersDto queryParameters); // Tüm kullanıcıları al ve sayfalama, sıralama yap
        Task<PagedResultDto<SimpleUserDto>> GetAllUsersWithPagingAsync(UserQueryParametersDto queryParameters); // Tüm kullanıcıları sayfalama ve sıralama ile al
        Task<SimpleUserDto> GetUserByIdAsync(int id); // Id'ye göre bir kullanıcı al
        Task<SimpleUserDto> CreateUserAsync(CreateUserDto dto); // Yeni bir kullanıcı yarat
        Task UpdateUserAsync(int id, UpdateUserDto dto); // Bir kullanıcıyı güncelle
        Task DeleteUserAsync(int id); // Bir kullanıcıyı sil
        Task<SimpleUserDto?> AuthenticateAsync(string username, string password); // Kullanıcı adı ve şifre ile kimlik doğrulama yap

    }
}
