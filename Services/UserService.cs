using Microsoft.EntityFrameworkCore;
using Eventify.Data;
using Eventify.Models;
using Eventify.DTOs;
using Eventify.Helpers;
using Azure.Core;
using Eventify.Helpers; 


namespace Eventify.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<PagedResultDto<SimpleUserDto>> GetAllUsersWithPagingAsync(UserQueryParametersDto queryParameters)
        {
            var query = _context.Users.AsQueryable(); // IQueryable<User> sorgu nesnesi oluşturuldu

            // Toplam kullanıcı sayısı (filtre sonrası)
            var totalCount = await query.CountAsync(); // Users tablosundaki toplam kullanıcı sayısını alır

            // Sıralama
            query = queryParameters.SortBy.ToLower() switch // sortBy parametresine göre sıralama yapılıyor
            {
                "username" => queryParameters.Descending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username), // "username" ise Descending'e göre artan veya azalan sıralama yapılıyor
                "email" => queryParameters.Descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email), // "email" ise Descending'e göre artan veya azalan sıralama yapılıyor
                _ => queryParameters.Descending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id) // Diğer durumlarda Id'ye göre Descending'e göre artan veya azalan sıralama yapılıyor
            };

            // Sayfalama
            query = query
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize)
                .Take(queryParameters.PageSize); // geride kalan sayfa sayısı * sayfa boyutu kadar kayıtı atla ve son noktadan itibaren sayfa boyutu kadar kaydı al

            var users = await query
                .Select(u => new SimpleUserDto // Projeksiyon ile SimpleUserDto'ya dönüştür
                {
                    Id = u.Id, // query'den gelen kullanıcı Id'sini simpleUserDto'ya aktar
                    Username = u.Username, // query'den gelen kullanıcı adını simpleUserDto'ya aktar
                    Email = u.Email, // query'den gelen email'i simpleUserDto'ya aktar
                    Role = u.Role // query'den gelen rolü simpleUserDto'ya aktar
                }).ToListAsync(); // Sorguyu çalıştır ve listeye dönüştür

            return new PagedResultDto<SimpleUserDto> // PagedResultDto oluştur
            {
                Items = users, // Dönüştürülmüş kullanıcı listesi
                Page = queryParameters.Page, // Sayfa numarası
                PageSize = queryParameters.PageSize, // Sayfa boyutu
                TotalCount = totalCount, // Users tablosundaki toplam kullanıcı sayısı
                HasMore = queryParameters.Page * queryParameters.PageSize < totalCount // Daha fazla kayıt var mı? (sayfa numarası * sayfa boyutu < toplam kullanıcı sayısı ise true, aksi halde false)
            };
        }

        public async Task<IEnumerable<SimpleUserDto>> GetAllUsersAsync(UserQueryParametersDto queryParameters)
        {
            var query = _context.Users.AsQueryable();
            var totalCount = await query.CountAsync(); ////////////////////////////////

            // Sıralama
            query = queryParameters.SortBy.ToLower() switch
            {
                "username" => queryParameters.Descending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username), //Sıralama parametresi username ise Descanding'e göre artan veya azalan sıralama yap
                "email" => queryParameters.Descending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email), //Sıralama parametresi email ise Descanding'e göre artan veya azalan sıralama yap
                _ => queryParameters.Descending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id) //Sıralama parametresi boş ise Descanding'e göre artan veya azalan sıralama yap
            };

            // Sayfalama
            query = query
                .Skip((queryParameters.Page - 1) * queryParameters.PageSize) // geride kalan sayfa sayısı * sayfa boyutu kadar kayıtı atla ve son noktadan itibaren sayfa boyutu kadar kaydı al
                .Take(queryParameters.PageSize);

            return await query
                .Select(user => new SimpleUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role 
                })
                .ToListAsync(); 
        }


        public async Task<SimpleUserDto> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id) // Id'ye göre filtrele
                .Select(u => new SimpleUserDto // Projeksiyon ile SimpleUserDto'ya dönüştür
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .FirstOrDefaultAsync(); // Id'ye göre ilk eşleşen kullanıcıyı al veya default değer döndür

            return user;
        }
        

        public async Task<SimpleUserDto> CreateUserAsync(CreateUserDto dto)
        {
            var user = new User // dto'dan gelen verilerle yeni bir kullanıcı oluştur
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password, // şifreleme yapılabilir
                Role = dto.Role // Kullanıcının rolünü ekle
            };

            await _context.Users.AddAsync(user); // Yeni kullanıcıyı ekle
            await _context.SaveChangesAsync(); // Değişiklikleri kaydet

            return new SimpleUserDto // Yeni oluşturulan kullanıcıyı Dto olarak döndür
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role // Kullanıcının rolünü ekle
            };
        }


        public async Task UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id); // Id'ye göre kullanıcıyı al
            if (user == null) // user boş mu
                throw new KeyNotFoundException($"User with id {id} not found."); // Eğer boşsa hata fırlat

            user.Username = dto.Username; // Dto'dan gelen verileri kayda geçir
            user.Email = dto.Email;
            user.Password = dto.Password; 
            user.Role = dto.Role; // Kullanıcının rolünü güncelle

            _context.Users.Update(user); // Kullanıcıyı güncelle
            await _context.SaveChangesAsync(); // Değişiklikleri kaydet
        }


        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id); // Id'ye göre kullanıcıyı al
            if (user == null) // user boş mu
            {
                throw new KeyNotFoundException($"User with id {id} not found."); // Eğer boşsa hata fırlat
            }

            _context.Users.Remove(user); // Kullanıcıyı veritabanından sil
            await _context.SaveChangesAsync(); // Değişiklikleri kaydet
        }

        
        public async Task<SimpleUserDto?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username == username && u.Password == password); // ⚠️ Parola hashleme önerilir

            if (user == null) return null;

            return new SimpleUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role // Kullanıcının rolünü ekle
            };
        }

        
    }
}
