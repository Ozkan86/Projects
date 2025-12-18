using Eventify.DTOs;
using Eventify.Services;
using Eventify.Helpers;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Eventify.Models;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Eventify.Controllers.V2
{
    [Authorize]
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService; // Katılım işlemleri servisi

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService; // Servis enjekte edilir
        }

        private string GetBaseUrl()
        {
            return $"{Request.Scheme}://{Request.Host}"; // Temel URL oluşturulur
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<AttendanceLinkDto>>> GetAllAttendances(
            [FromQuery] string? sortBy = "id",
            [FromQuery] bool isDescending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var baseUrl = GetBaseUrl(); // Temel URL alınır
            var pagedResult = await _attendanceService.GetAllAttendancesPagedAsync(sortBy, isDescending, page, pageSize); // Tüm katılım kayıtlarını sayfalı olarak getir
            var attendanceDtos = pagedResult.Items.Select(attendance => new AttendanceLinkDto // Katılım kayıtlarını AttendanceLinkDto'ya dönüştür (projeksiyon)
            {
                Id = attendance.Id, 
                IsAttending = attendance.IsAttending,
                Event = new SimpleEventDto // İlgili etkinliğin temel bilgileri
                {
                    Id = attendance.Event.Id,
                    Name = attendance.Event.Name,
                    Location = attendance.Event.Location,
                    Date = attendance.Event.Date
                },
                User = new SimpleUserDto // İlgili kullanıcının temel bilgileri
                {
                    Id = attendance.User.Id,
                    Username = attendance.User.Username,
                    Email = attendance.User.Email,
                    Role = attendance.User.Role
                },
                Links = AttendanceLinkHelper.GenerateLinks(attendance.Id, baseUrl) // Linkler burada oluşturuluyor
            }).ToList();
            var result = new PagedResultDto<AttendanceLinkDto>
            {
                Items = attendanceDtos,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,
                HasMore = pagedResult.HasMore
            };

            return Ok(result); // 200 OK ile PagedResultDto döndürülür
        }
        

        [Authorize(Roles = "Admin,User")] // Hem admin hem de user rolü ile erişime izin verildi
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendance(int id)
        {
            var result = await _attendanceService.GetAttendanceByIdAsync(id); // ID ile katılım kaydını getir
            if (result == null)
                return NotFound(); // Kayıt yoksa 404 döner

            var baseUrl = GetBaseUrl();
            var links = AttendanceLinkHelper.GenerateLinks(id, baseUrl); // Linkler burada oluşturuluyor

            return Ok(new { result, Links = links }); // Kayıt ve linkler döner
        }

        [Authorize(Roles = "Admin")] // Sadece admin rolü ile erişime izin verildi
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] UpdateAttendanceDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Model doğrulama hatası varsa 400 döner
            }

            var updatedAttendance = await _attendanceService.UpdateAttendanceAsync(id, updateDto); // Katılım kaydını güncelle
            Log.Information("Katılım güncellendi. Katılım ID: {AttendanceId}, Katılım Durumu: {IsAttending}", updatedAttendance.Id, updatedAttendance.IsAttending); // Loglama yapıldı
            if (updatedAttendance == null)
                return NotFound(); // Kayıt yoksa 404 döner

            var baseUrl = GetBaseUrl();
            var links = AttendanceLinkHelper.GenerateLinks(id, baseUrl); // Linkler burada oluşturuluyor

            return Ok(new { updatedAttendance, Links = links }); // Güncellenen kayıt ve linkler döner
        }

        [Authorize(Roles = "Admin")] // Sadece admin rolü ile erişime izin verildi
        [HttpPost]
        public async Task<ActionResult<Attendance>> CreateAttendance([FromBody] CreateAttendanceDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Model doğrulama hatası varsa 400 döner
            }

            var createdAttendance = await _attendanceService.CreateAttendanceAsync(createDto); // Yeni katılım kaydı oluştur
            Log.Information("Yeni katılım oluşturuldu. Katılım ID: {AttendanceId}, Katılım Durumu: {IsAttending}", createdAttendance.Id, createdAttendance.IsAttending); // Loglama yapıldı
            var baseUrl = GetBaseUrl();
            var links = AttendanceLinkHelper.GenerateLinks(createdAttendance.Id, baseUrl); // Linkler burada oluşturuluyor

            return CreatedAtAction(nameof(GetAttendance), new { id = createdAttendance.Id }, new { createdAttendance, Links = links }); // 201 Created ve yeni kayıt döner
        }

        [Authorize(Roles = "Admin")] // Sadece admin rolü ile erişime izin verildi
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try
            {
                await _attendanceService.DeleteAttendanceAsync(id); // Katılım kaydını sil
                Log.Information("Katılım silindi. Katılım ID: {AttendanceId}", id); // Loglama yapıldı
                return NoContent(); // Silme işlemi başarılı, 204 döner
            }
            catch (ArgumentOutOfRangeException)
            {
                Log.Error("Katılım silinemedi. Katılım ID: {Id}", id); // Hata logu
                return NotFound(); // Kayıt yoksa 404 döner
            }
        }
    }
}



/*[Authorize(Roles = "Admin,User")] // Hem admin hem de user rolü ile erişime izin verildi
        [HttpGet]
        public async Task<IActionResult> GetAllAttendances(
            [FromQuery] string? sortBy = "id", // Sıralama alanı
            [FromQuery] bool isDescending = false, // Azalan sıralama
            [FromQuery] int page = 1, // Sayfa numarası
            [FromQuery] int pageSize = 10) // Sayfa başına kayıt
        {
            var attendances = await _attendanceService.GetAllAttendancesAsync(sortBy, isDescending, page, pageSize); // Tüm katılım kayıtlarını getir
            var baseUrl = GetBaseUrl();

            var attendanceDtos = attendances.Select(attendance => new AttendanceLinkDto
            {
                Id = attendance.Id,
                IsAttending = attendance.IsAttending,
                Event = new SimpleEventDto
                {
                    Id = attendance.Event.Id,
                    Name = attendance.Event.Name,
                    Location = attendance.Event.Location,
                    Date = attendance.Event.Date
                },
                User = new SimpleUserDto
                {
                    Id = attendance.User.Id,
                    Username = attendance.User.Username,
                    Email = attendance.User.Email,
                    Role = attendance.User.Role
                },
                Links = AttendanceLinkHelper.GenerateLinks(attendance.Id, baseUrl) // Linkler burada oluşturuluyor
            });

            return Ok(attendanceDtos); // 200 OK ve liste döner
        }*/