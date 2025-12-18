using Microsoft.AspNetCore.Mvc;
using Eventify.Models;
using Eventify.Services;
using Eventify.DTOs;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Serilog;
using Eventify.Helpers;

namespace Eventify.Controllers
{
    
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService; // Kullanıcı işlemleri servisi

        public UsersController(IUserService userService)
        {
            _userService = userService; // Servis dependency injection ile alınır
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<UserDto>>> GetUsers([FromQuery] UserQueryParametersDto queryParameters)
        {
            var pagedResult = await _userService.GetAllUsersWithPagingAsync(queryParameters); // Kullanıcıları getir

            var userDtos = pagedResult.Items.Select(u => new UserDto // PagedResultDto içindeki kullanıcıları UserDto'ya dönüştür (projeksiyon)
            {
                Id = u.Id, // ItemDto'daki id'yi UserDto'ya aktar
                Name = u.Username, // ItemDto'daki kullanıcı adını UserDto'ya aktar
                Email = u.Email, // ItemDto'daki email'i UserDto'ya aktar
                Role = u.Role, // ItemDto'daki rolü UserDto'ya aktar
            }).ToList(); // Listeye dönüştür

            var result = new PagedResultDto<UserDto> // PagedResultDto oluştur
            {
                Items = userDtos, // Dönüştürülmüş kullanıcı listesi
                Page = pagedResult.Page, // Sayfa numarası
                PageSize = pagedResult.PageSize, // Sayfa boyutu
                TotalCount = pagedResult.TotalCount, // Users tablosundaki toplam kullanıcı sayısı
                HasMore = pagedResult.HasMore // Daha fazla kayıt var mı?
            };

            return Ok(result); // 200 OK ve kullanıcı listesi döner
        }


        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<ActionResult<SimpleUserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id); // ID ile kullanıcıyı getir
            if (user == null)
            {
                return NotFound(); // Kullanıcı yoksa 404 döner
            }
            return Ok(user); // Kullanıcı bulunduysa 200 OK döner
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<SimpleUserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Model doğrulama hatası varsa 400 döner
            }
            var createdUser = await _userService.CreateUserAsync(createUserDto); // Yeni kullanıcı oluştur
            Log.Information("Yeni kullanıcı oluşturuldu. Kullanıcı adı: {Username}, Email: {Email}", createdUser.Username, createdUser.Email); // Loglama yapıldı
            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser); // 201 Created ve yeni kullanıcı döner
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Model doğrulama hatası varsa 400 döner
            }
            try
            {
                await _userService.UpdateUserAsync(id, dto); // Kullanıcıyı güncelle
                Log.Information("Kullanıcı güncellendi. Kullanıcı ID: {Id}, Kullanıcı adı: {Username}, Email: {Email}", id, dto.Username, dto.Email); // Loglama yapıldı
                return NoContent(); // Başarılıysa 204 döner
            }
            catch (KeyNotFoundException)
            {
                Log.Error("Kullanıcı bulunamadı. Id: {Id}", id); // Hata logu
                return NotFound(); // Kullanıcı yoksa 404 döner
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value); // Kullanıcı rolleri alınır
            var rolesString = string.Join(", ", roles); // Rolleri virgülle birleştirir
            Log.Information("DeleteUser çağrıldı. Kullanıcı rolleri: {Roles}", rolesString); // Loglama yapıldı
            var user = await _userService.GetUserByIdAsync(id); // Silinecek kullanıcıyı getir
            if (user == null)
            {
                return NotFound(); // Kullanıcı yoksa 404 döner
            }

            await _userService.DeleteUserAsync(id); // Kullanıcıyı sil
            return NoContent(); // Başarılıysa 204 döner
        }
    }
}
