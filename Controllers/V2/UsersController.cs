using Asp.Versioning;
using Eventify.DTOs;
using Eventify.Helpers;
using Eventify.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Eventify.Controllers.V2
{
    [Authorize]
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService; // Kullanıcı işlemleri servisi

        public UsersController(IUserService userService)
        {
            _userService = userService; // Servis dependency injection ile alınır
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<UserLinkDto>>> GetUsers([FromQuery] UserQueryParametersDto queryParameters)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}"; // API'nin temel URL'si (örn: http://localhost:5000/api/v2/users)

            var pagedResult = await _userService.GetAllUsersWithPagingAsync(queryParameters); // Kullanıcıları getir ve sayfalama, sıralama yap

            var userDtos = pagedResult.Items.Select(u => new UserLinkDto // PagedResultDto içindeki kullanıcıları UserLinkDto'ya dönüştür (projeksiyon)
            {
                Id = u.Id, // Items'daki id'yi UserLinkDto'ya aktar
                Name = u.Username, // Items'daki kullanıcı adını UserLinkDto'ya aktar
                Email = u.Email, // Items'daki email'i UserLinkDto'ya aktar
                Role = u.Role, // Items'daki rolü UserLinkDto'ya aktar
                Links = UserLinkHelper.GenerateLinks(u.Id, baseUrl) // HATEOAS linklerini oluştur
            }).ToList(); // Listeye dönüştür

            var result = new PagedResultDto<UserLinkDto> // PagedResultDto oluştur
            {
                Items = userDtos, // Dönüştürülmüş kullanıcı listesi
                Page = pagedResult.Page, // Sayfa numarası
                PageSize = pagedResult.PageSize, // Sayfa boyutu
                TotalCount = pagedResult.TotalCount, // Users tablosundaki toplam kullanıcı sayısı
                HasMore = pagedResult.HasMore // Daha fazla kayıt var mı?
            };

            return Ok(result); // 200 OK ve kullanıcı listesi döner
        }


        [Authorize(Roles = "Admin,User")] // Hem admin hem de user rolü ile erişime izin verildi
        [HttpGet("{id}")]
        public async Task<ActionResult<UserLinkDto>> GetUser(int id)
        {
            // ID ile kullanıcıyı getir ve HATEOAS linkleri ekle
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(); // Kullanıcı yoksa 404 döner
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var userDto = new UserLinkDto
            {
                Id = user.Id,
                Name = user.Username,
                Email = user.Email,
                Role = user.Role,
                Links = UserLinkHelper.GenerateLinks(user.Id, baseUrl)
            };

            return Ok(userDto); // Kullanıcı bulunduysa 200 OK döner
        }

        [Authorize(Roles = "Admin")] // Sadece admin rolü ile erişime izin verildi
        [HttpPost]
        public async Task<ActionResult<UserLinkDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            // Yeni kullanıcı oluştur
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Model doğrulama hatası varsa 400 döner
            }
            var createdUser = await _userService.CreateUserAsync(createUserDto);
            Log.Information("Yeni kullanıcı oluşturuldu. Kullanıcı adı: {Username}, Email: {Email}", createdUser.Username, createdUser.Email);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var userDto = new UserLinkDto
            {
                Id = createdUser.Id,
                Name = createdUser.Username,
                Email = createdUser.Email,
                Links = UserLinkHelper.GenerateLinks(createdUser.Id, baseUrl)
            };

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto); // 201 Created ve yeni kullanıcı döner
        }

        [Authorize(Roles = "Admin")] // Sadece admin rolü ile erişime izin verildi
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            // Kullanıcıyı güncelle
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Model doğrulama hatası varsa 400 döner
            }

            try
            {
                await _userService.UpdateUserAsync(id, updateUserDto);
                Log.Information("Kullanıcı güncellendi. Kullanıcı adı: {Username}, Email: {Email}", updateUserDto.Username, updateUserDto.Email);
                return NoContent(); // Başarılıysa 204 döner
            }
            catch (KeyNotFoundException)
            {
                Log.Error("Kullanıcı güncellenemedi. Kullanıcı ID: {Id}", id);
                return NotFound(); // Kullanıcı yoksa 404 döner
            }
        }

        [Authorize(Roles = "Admin")] // Sadece admin rolü ile erişime izin verildi
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Kullanıcıyı sil
            var user = await _userService.GetUserByIdAsync(id);
            Log.Information("Kullanıcı silindi. Kullanıcı adı: {Username}, Email: {Email}", user.Username, user.Email);
            if (user == null)
            {
                return NotFound(); // Kullanıcı yoksa 404 döner
            }

            await _userService.DeleteUserAsync(id);
            return NoContent(); // Başarılıysa 204 döner
        }
    }
}



/*[Authorize(Roles = "Admin,User")] // Hem admin hem de user rolü ile erişime izin verildi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserLinkDto>>> GetUsers([FromQuery] UserQueryParametersDto queryParameters)
        {
            // Kullanıcıları getir ve HATEOAS linkleri ekle
            var users = await _userService.GetAllUsersAsync(queryParameters);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var userDtos = users.Select(u => new UserLinkDto
            {
                Id = u.Id,
                Name = u.Username,
                Email = u.Email,
                Role = u.Role,
                Links = UserLinkHelper.GenerateLinks(u.Id, baseUrl)
            }).ToList();

            return Ok(userDtos); // 200 OK ve kullanıcı listesi döner
        }*/