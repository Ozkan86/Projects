using Microsoft.AspNetCore.Mvc;
using Eventify.Models;
using Eventify.Services;
using Eventify.DTOs;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace Eventify.Controllers
{
    
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService) //kurucu enjeksiyonu
        {
            _eventService = eventService;
        }


        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<EventDto>>> GetEvents(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isDescending = false)
        {
            var pagedResult = await _eventService.GetAllEventsWithPagingAsync(pageNumber, pageSize, sortBy, isDescending);

            return Ok(pagedResult);
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id) //rota verilerinden model bağlama
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            return Ok(eventEntity);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdEvent = await _eventService.CreateEventAsync(createDto); // DTO'yu service'e gönder
            Log.Information("Yeni etkinlik oluşturuldu. Etkinlik adı: {Name}, Konum: {Location}, Tarih: {Date}", createdEvent.Name, createdEvent.Location, createdEvent.Date); // Loglama yapıldı
            return CreatedAtAction(nameof(GetEvent), new { id = createdEvent.Id }, createdEvent);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _eventService.UpdateEventAsync(updateDto,id);
            Log.Information("Etkinlik güncellendi. Etkinlik adı: {Name}, Konum: {Location}, Tarih: {Date}", updateDto.Name, updateDto.Location, updateDto.Date); // Loglama yapıldı

            if (!updated)
            {
                return NotFound(new { message = $"Event with id {id} not found." });
            }

            return NoContent(); // 204
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventEntity = await _eventService.GetEventByIdAsync(id);

            if (eventEntity == null)
            {
                return NotFound(new { message = $"Event with id {id} not found." });
            }

            await _eventService.DeleteEventAsync(id);
            Log.Information("Etkinlik silindi. Etkinlik adı: {Name}, Konum: {Location}, Tarih: {Date}", eventEntity.Name, eventEntity.Location, eventEntity.Date); // Loglama yapıldı
            return NoContent(); // 204 No Content: İşlem başarılı ancak herhangi bir veri döndürülmez
        }

     
    }
}
