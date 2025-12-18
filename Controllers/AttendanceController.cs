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
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService; // Katılım işlemleri servisi

        public AttendanceController(IAttendanceService attendanceService) // Kurucu enjeksiyonu
        {
            _attendanceService = attendanceService; // Servis enjekte edilir
        }

        [Authorize(Roles = "Admin,User")] // Admin ve User rolleri erişebilir
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<SimpleAttendanceDto>>> GetAllAttendances(
            [FromQuery] string? sortBy = "id", // Sıralama alanı
            [FromQuery] bool isDescending = false, // Azalan sıralama
            [FromQuery] int page = 1, // Sayfa numarası
            [FromQuery] int pageSize = 10) // Sayfa başına kayıt
        {
            var attendances = await _attendanceService.GetAllAttendancesPagedAsync(sortBy, isDescending, page, pageSize); // Tüm katılım kayıtlarını getir
            return Ok(attendances); // 200 OK ve liste döner
        }
        

        [Authorize(Roles = "Admin")] // Sadece admin erişebilir
        [HttpPost]
        public async Task<ActionResult<AttendanceDetailsDto>> CreateAttendance([FromBody] CreateAttendanceDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Model doğrulama hatası varsa 400 döner
            }
            var createdAttendance = await _attendanceService.CreateAttendanceAsync(createDto); // Yeni katılım kaydı oluştur
            Log.Information("Yeni katılım kaydı oluşturuldu. Kullanıcı ID: {UserId}, Etkinlik ID: {EventId}", createdAttendance.UserId, createdAttendance.EventId); // Loglama yapıldı
            return CreatedAtAction(nameof(GetAttendance), new { id = createdAttendance.Id }, createdAttendance); // 201 Created ve yeni kayıt döner
        }

        [Authorize(Roles = "Admin,User")] // Admin ve User rolleri erişebilir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendance(int id)
        {
            var result = await _attendanceService.GetAttendanceByIdAsync(id); // ID ile katılım kaydını getir
            if (result == null)
                return NotFound(); // Kayıt yoksa 404 döner

            return Ok(result); // Kayıt bulunduysa 200 OK döner
        }

        [Authorize(Roles = "Admin")] // Sadece admin erişebilir
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, UpdateAttendanceDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Model geçersizse 400 döner
            }

            var updatedAttendance = await _attendanceService.UpdateAttendanceAsync(id, updateDto); // Katılım kaydını güncelle
            Log.Information("Katılım kaydı güncellendi."); // Loglama yapıldı

            if (updatedAttendance == null) // Güncellenmiş kayıt yoksa
            {
                return NotFound(); // 404 döner
            }

            return Ok(updatedAttendance); // Başarılıysa 200 OK döner
        }

        [Authorize(Roles = "Admin")] // Sadece admin erişebilir
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try
            {
                await _attendanceService.DeleteAttendanceAsync(id); // Katılım kaydını sil
                Log.Information("Katılım kaydı silindi. ID: {Id}", id); // Loglama yapıldı
                return NoContent(); // Silme işlemi başarılı, 204 döner
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error("Attendance silinemedi. Hata: {Message}", ex.Message); // Loglama yapıldı
                return NotFound(ex.Message); // Kayıt yoksa 404 döner
            }
            catch (ArgumentException)
            {
                return NotFound(); // Kayıt yoksa 404 döner
            }
        }
    }
}




/*[Authorize(Roles = "Admin,User")] // Admin ve User rolleri erişebilir
        [HttpGet]
        public async Task<IActionResult> GetAllAttendances(
            [FromQuery] string? sortBy = "id", // Sıralama alanı
            [FromQuery] bool isDescending = false, // Azalan sıralama
            [FromQuery] int page = 1, // Sayfa numarası
            [FromQuery] int pageSize = 10) // Sayfa başına kayıt
        {
            var attendances = await _attendanceService.GetAllAttendancesAsync(sortBy, isDescending, page, pageSize); // Tüm katılım kayıtlarını getir
            return Ok(attendances); // 200 OK ve liste döner
        }*/