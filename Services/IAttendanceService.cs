using Eventify.Models;
using Eventify.DTOs;

namespace Eventify.Services
{
    // Katılım (Attendance) işlemlerini tanımlayan arayüz
    public interface IAttendanceService
    {
        Task<PagedResultDto<SimpleAttendanceDto>> GetAllAttendancesPagedAsync(string? sortBy, bool isDescending, int page, int pageSize); // Tüm katılımları sayfalı olarak getir
        Task<IEnumerable<SimpleAttendanceDto>> GetAllAttendancesAsync(string? sortBy, bool isDescending, int page, int pageSize); // Tüm katılımları getir
        Task<AttendanceDetailsDto> CreateAttendanceAsync(CreateAttendanceDto createDto); // Yeni bir katılım kaydı oluştur
        Task<SimpleAttendanceDto> GetAttendanceByIdAsync(int id);   // ID'ye göre katılım kaydını getir
        Task<AttendanceDetailsDto> UpdateAttendanceAsync(int id, UpdateAttendanceDto updateDto); // Katılım kaydını güncelle
        Task DeleteAttendanceAsync(int id);                // ID'ye göre katılım kaydını sil
    }
}
