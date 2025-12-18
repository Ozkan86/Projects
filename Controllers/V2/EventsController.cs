using Microsoft.AspNetCore.Mvc;
using Eventify.Models;
using Eventify.Services;
using Eventify.DTOs;
using Asp.Versioning;
using Eventify.Helpers;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace Eventify.Controllers.V2
{
    [Authorize]
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService) //Kurucu enjeksiyonu
        {
            _eventService = eventService;
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<EventLinkDto>>> GetEvents(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool isDescending = false)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; //HATEOAS için oluşturulacak linklerin şema ve host bilgisi alınır

            var pagedResult = await _eventService.GetAllEventsWithPagingAsync(pageNumber, pageSize, sortBy, isDescending); //Sayfalama ve sıralama ile tüm etkinlikleri al

            var eventDtos = pagedResult.Items.Select(e => new EventLinkDto //Projection ile EventDto'dan EventLinkDto'ya dönüştürülüyor
            {
                Id = e.Id, // Id'yi Items'dan EventLinkDto'ya atıyoruz
                Name = e.Name, // İsmi Items'dan EventLinkDto'ya atıyoruz
                Location = e.Location, // Konumu Items'dan EventLinkDto'ya atıyoruz
                Date = e.Date, // Tarihi Items'dan EventLinkDto'ya atıyoruz
                Attendances = e.Attendances, // Katılımları Items'dan EventLinkDto'ya atıyoruz
                Links = EventLinkHelper.GenerateLinks(e.Id, baseUrl) //EventLinkHelper sınıfı ile HATEOAS linkleri oluşturuluyor
            }).ToList(); // Listeye ekleniyor

            var result = new PagedResultDto<EventLinkDto>
            {
                Items = eventDtos, // EventLinkDto'ları Items'a atıyoruz
                Page = pagedResult.Page, // Sayfa numarasını atıyoruz
                PageSize = pagedResult.PageSize, // Sayfa boyutunu atıyoruz
                TotalCount = pagedResult.TotalCount, // Toplam kayıt sayısını atıyoruz
                HasMore = pagedResult.HasMore // Daha fazla kayıt var mı bilgisini atıyoruz
            };

            return Ok(result); // 200 OK: İşlem başarılı ve veriler döndürülüyor
        }


        [Authorize(Roles = "Admin,User")] //Hem admin hem de user rolü ile erişime izin verildi
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventLinkDto>> GetEvent(int id) //rota verilerinden model bağlama
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);
            if (eventEntity == null) //Olay yoksa null döner
            {
                return NotFound();
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}"; //HATEOAS için oluşturulacak linklerin şema ve host bilgisi alınır

            var eventDto = new EventLinkDto
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Location = eventEntity.Location,
                Date = eventEntity.Date,
                Attendances = eventEntity.Attendances,
                Links = EventLinkHelper.GenerateLinks(eventEntity.Id, baseUrl) //EventLinkHelper sınıfı ile HATEOAS linkleri oluşturuluyor
            };

            return Ok(eventDto);
        }

        [Authorize(Roles = "Admin")] //Sadece admin rolü ile erişime izin verildi
        [HttpPost]
        public async Task<ActionResult<EventLinkDto>> CreateEvent([FromBody] CreateEventDto createDto)
        {
            if(!ModelState.IsValid) //Model doğrulaması
            {
                return BadRequest(ModelState);
            }
            var createdEvent = await _eventService.CreateEventAsync(createDto); // DTO'yu service'e gönder(geriye yaratılan event'i döner)
            Log.Information("Yeni etkinlik oluşturuldu. Etkinlik adı: {EventName}, Konum: {Location}, Tarih: {Date}", createdEvent.Name, createdEvent.Location, createdEvent.Date); //Loglama yapıldı
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; //HATEOAS için oluşturulacak linklerin şema ve host bilgisi alınır

            var eventDto = new EventLinkDto
            {
                Id = createdEvent.Id,
                Name = createdEvent.Name,
                Location = createdEvent.Location,
                Date = createdEvent.Date,
                Attendances = createdEvent.Attendances,
                Links = UserLinkHelper.GenerateLinks(createdEvent.Id, baseUrl)
            };
            return CreatedAtAction(nameof(GetEvent), new { id = createdEvent.Id }, eventDto);
        }


        [Authorize(Roles = "Admin")] //Sadece admin rolü ile erişime izin verildi
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var updated = await _eventService.UpdateEventAsync(updateDto,id);
            Log.Information("Etkinlik güncellendi. Etkinlik adı: {EventName}, Konum: {Location}, Tarih: {Date}", updateDto.Name, updateDto.Location, updateDto.Date); //Loglama yapıldı

            if (!updated)
            {
                return NotFound(new { message = $"Event with id {id} not found." });
            }

            return NoContent(); // 204
        }


        [Authorize(Roles = "Admin")] //Sadece admin rolü ile erişime izin verildi
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);

            if (eventEntity == null)
            {
                return NotFound(new { message = $"Event with id {id} not found." });
            }

            await _eventService.DeleteEventAsync(id);
            Log.Information("Etkinlik silindi. Etkinlik adı: {EventName}, Konum: {Location}, Tarih: {Date}", eventEntity.Name, eventEntity.Location, eventEntity.Date); //Loglama yapıldı
            return NoContent(); // 204 No Content: İşlem başarılı ancak herhangi bir veri döndürülmez
        }

    }
}




/*[Authorize(Roles = "Admin,User")] //Hem admin hem de user rolü ile erişime izin verildi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventLinkDto>>> GetEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isDescending = false)
        {
            var events = await _eventService.GetAllEventsAsync(pageNumber, pageSize, sortBy, isDescending);
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; //HATEOAS için oluşturulacak linklerin şema ve host bilgisi alınır
            var eventDtos = events.Select(e => new EventLinkDto //Projection ile EventDto'dan EventLinkDto'ya dönüştürülüyor
            {
                Id = e.Id,
                Name = e.Name,
                Location = e.Location,
                Date = e.Date,
                Attendances = e.Attendances,
                Links = EventLinkHelper.GenerateLinks(e.Id, baseUrl) //EventLinkHelper sınıfı ile HATEOAS linkleri oluşturuluyor
            }).ToList();
            return Ok(eventDtos);
        }*/
