using Eventify.DTOs; //DTOs klasörü kullanılıyor
using Eventify.Models; //models klasörü kullanılıyor

namespace Eventify.Services //bulunduğumuz dizin
{
    public interface IEventService
    {
        Task<PagedResultDto<EventDto>> GetAllEventsWithPagingAsync(int pageNumber, int pageSize, string? sortBy, bool isDescending); //Sayfalama ile tüm olayları al
        Task<IEnumerable<EventDto>> GetAllEventsAsync(int pageNumber, int pageSize, string? sortBy, bool isDescending); //tüm olayları al
        Task<EventDto?> GetEventByIdAsync(int id); //Id'ye göre bir tane olay al
        Task<EventDto> CreateEventAsync(CreateEventDto dto); //olay yarat
        Task<bool> UpdateEventAsync(UpdateEventDto updateDto,int id); //bir olayı güncelle
        Task DeleteEventAsync(int id); //bir olayı sil
    }
}
